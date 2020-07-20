using UnityEngine;
using Unity.Networking.Transport;
using Assets.Code;
using Unity.Jobs;
using UnityEngine.Timers;
using System.Collections.Generic;

public class ClientBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NetworkConnection connection;

    private JobHandle networkJobHandle;

    private Queue<MessageHeader> ClientMessagesQueue;
    public MessageEvent[] ClientCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];
    
    // player name
    public string playerName;

    // Use this for initialization
    void Start()
    {
        networkDriver = NetworkDriver.Create();
        connection = default;

        ClientMessagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ClientCallbacks.Length; i++)
        {
            ClientCallbacks[i] = new MessageEvent();
        }

        ClientCallbacks[(int)MessageHeader.MessageType.NewPlayer].AddListener(PlayerManager.Instance.NewPlayer);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        connection = networkDriver.Connect(endpoint);
        TimerManager.Instance.AddTimer(StayAlive, 10);
    }

    private void StayAlive()
    {
        networkJobHandle.Complete();
        NetworkManager.SendMessage(networkDriver, new StayAliveMessage(), connection);
    }

    // Update is called once per frame
    void Update()
    {
        networkJobHandle.Complete();

        if(!connection.IsCreated)
            return;

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty)
        {
            if(cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to server");
            }
            else if(cmd == NetworkEvent.Type.Data)
            {
                var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                switch (messageType)
                {
                    case MessageHeader.MessageType.None:
                        var stayAliveMessage = NetworkManager.ReadMessage<StayAliveMessage>(reader, ClientMessagesQueue) as StayAliveMessage;
                        TimerManager.Instance.AddTimer(StayAlive, 10);
                        break;
                    case MessageHeader.MessageType.NewPlayer:
                        NewPlayerMessage newPlayerMessage = (NewPlayerMessage)NetworkManager.ReadMessage<NewPlayerMessage>(reader, ClientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.Welcome:
                        var welcomeMessage = (WelcomeMessage)NetworkManager.ReadMessage<WelcomeMessage>(reader,ClientMessagesQueue) as WelcomeMessage;

                        var setNameMessage = new SetNameMessage
                        {
                            Name = playerName
                        };

                        PlayerManager.Instance.CurrentPlayer = new Players(welcomeMessage.PlayerID, playerName, welcomeMessage.Colour);
                        UIManager.Instance.SpawnPlayerLabel(PlayerManager.Instance.CurrentPlayer);
                        NetworkManager.SendMessage(networkDriver, setNameMessage, connection);
                        break;
                    case MessageHeader.MessageType.SetName:
                        break;
                    case MessageHeader.MessageType.RequestDenied:
                        break;
                    case MessageHeader.MessageType.PlayerLeft:
                        break;
                    case MessageHeader.MessageType.StartGame:
                        break;
                    case MessageHeader.MessageType.Count:
                        break;
                    default:
                        break;
                }
            }
            else if(cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void ProcessMessagesQueue()
    {
        while (ClientMessagesQueue.Count > 0)
        {
            var message = ClientMessagesQueue.Dequeue();
            ClientCallbacks[(int)message.Type].Invoke(message);
        }
    }


    private void OnDestroy()
    {
        networkDriver.Dispose();
    }
}
