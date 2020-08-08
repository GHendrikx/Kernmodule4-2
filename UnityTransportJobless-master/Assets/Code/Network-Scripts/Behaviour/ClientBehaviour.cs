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

    private Queue<MessageHeader> clientMessagesQueue;
    public MessageEvent[] ClientCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count];

    // player name
    public string playerName;
    public string IPAdress;

    // Use this for initialization
    private void Start()
    {
        networkDriver = NetworkDriver.Create();
        connection = default;

        clientMessagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ClientCallbacks.Length; i++)
        {
            ClientCallbacks[i] = new MessageEvent();
        }

        ClientCallbacks[(int)MessageHeader.MessageType.NewPlayer].AddListener(PlayerManager.Instance.NewPlayer);
        ClientCallbacks[(int)MessageHeader.MessageType.NewPlayer].AddListener(UIManager.Instance.SortingPlayerLabel);
        ClientCallbacks[(int)MessageHeader.MessageType.StartGame].AddListener(PlayerManager.Instance.SetBeginHealth);
        ClientCallbacks[(int)MessageHeader.MessageType.StartGame].AddListener(UIManager.Instance.SwitchToGamePanel);
        ClientCallbacks[(int)MessageHeader.MessageType.StartGame].AddListener(UIManager.Instance.DeletePlayerLabels);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerTurn].AddListener(UIManager.Instance.CheckTurn);
        ClientCallbacks[(int)MessageHeader.MessageType.RequestDenied].AddListener(UIManager.Instance.ShowErrorCode);
        ClientCallbacks[(int)MessageHeader.MessageType.RoomInfo].AddListener(UIManager.Instance.ShowNewRoom);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerEnterRoom].AddListener(UIManager.Instance.EnterPlayer);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeaveRoom].AddListener(UIManager.Instance.LeavePlayer);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(UIManager.Instance.DisableSpawnLabel);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(UIManager.Instance.RemovePlayer);
        ClientCallbacks[(int)MessageHeader.MessageType.HitMonster].AddListener(UIManager.Instance.ToggleAttackAnimation);
        ClientCallbacks[(int)MessageHeader.MessageType.HitByMonster].AddListener(PlayerManager.Instance.HitByMonster);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerDies].AddListener(PlayerManager.Instance.PlayerDies);
        ClientCallbacks[(int)MessageHeader.MessageType.ObtainTreasure].AddListener(UIManager.Instance.ObtainTreasure);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerDefends].AddListener(UIManager.Instance.PlayerDefend);
        ClientCallbacks[(int)MessageHeader.MessageType.EndGame].AddListener(GameManager.Instance.InsertScore);

        if (GameManager.Instance.LOCAL)
        {
            var endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = 9000;
            connection = networkDriver.Connect(endpoint);
        }
        else
        {
            var endPoint = NetworkEndPoint.Parse(IPAdress,9000);
            connection = networkDriver.Connect(endPoint);
        }
        TimerManager.Instance.AddTimer(StayAlive, 10);
    }

    private void StayAlive()
    {
        networkJobHandle.Complete();
        NetworkManager.SendMessage(networkDriver, new StayAliveMessage(), connection);
    }

    // Update is called once per frame
    private void Update()
    {
        networkJobHandle.Complete();

        if (!connection.IsCreated)
            return;

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {

            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                switch (messageType)
                {
                    case MessageHeader.MessageType.None:
                        var stayAliveMessage = NetworkManager.ReadMessage<StayAliveMessage>(reader, clientMessagesQueue) as StayAliveMessage;
                        TimerManager.Instance.AddTimer(StayAlive, 10);
                        break;
                    case MessageHeader.MessageType.NewPlayer:
                        NewPlayerMessage newPlayerMessage = (NewPlayerMessage)NetworkManager.ReadMessage<NewPlayerMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.Welcome:
                        var welcomeMessage = (WelcomeMessage)NetworkManager.ReadMessage<WelcomeMessage>(reader, clientMessagesQueue) as WelcomeMessage;

                        var setNameMessage = new SetNameMessage
                        {
                            Name = playerName
                        };

                        Players currentPlayer = new Players(welcomeMessage.PlayerID, playerName, welcomeMessage.Colour);
                        PlayerManager.Instance.CurrentPlayer = currentPlayer;
                        UIManager.Instance.SpawnPlayerLabel(currentPlayer);
                        PlayerManager.Instance.SpawnSprite(currentPlayer);
                        NetworkManager.SendMessage(networkDriver, setNameMessage, connection);
                        break;

                    case MessageHeader.MessageType.RequestDenied:
                        NetworkManager.ReadMessage<RequestDeniedMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerLeft:
                        NetworkManager.ReadMessage<PlayerLeftMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.StartGame:
                        PlayerManager.Instance.SortingPlayerList();
                        NetworkManager.ReadMessage<StartGameMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerTurn:
                        NetworkManager.ReadMessage<PlayerTurnMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.RoomInfo:
                        NetworkManager.ReadMessage<RoomInfoMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerEnterRoom:
                        NetworkManager.ReadMessage<PlayerLeaveRoomMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerLeaveRoom:
                        NetworkManager.ReadMessage<PlayerLeaveRoomMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.ObtainTreasure:
                        NetworkManager.ReadMessage<ObtainTreasureMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.HitMonster:
                        NetworkManager.ReadMessage<HitMonsterMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.HitByMonster:
                        NetworkManager.ReadMessage<HitByMonsterMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerDefends:
                        NetworkManager.ReadMessage<PlayerDefendsMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerLeftDungeon:
                        NetworkManager.ReadMessage<PlayerLeftDungeonMessage>(reader, clientMessagesQueue);
                        break;
                    case MessageHeader.MessageType.PlayerDies:
                        NetworkManager.ReadMessage<PlayerDiesMessage>(reader, clientMessagesQueue);
                        connection.Disconnect(networkDriver);
                        break;
                    case MessageHeader.MessageType.EndGame:
                        NetworkManager.ReadMessage<EndGameMessage>(reader,clientMessagesQueue);
                        break;
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server");
                UIManager.Instance.DisconnectedPanel();
                connection = default;
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void ProcessMessagesQueue()
    {
        while (clientMessagesQueue.Count > 0)
        {
            var message = clientMessagesQueue.Dequeue();
            ClientCallbacks[(int)message.Type].Invoke(message);
        }
    }

    public void DisconnectPlayer()
    {
        networkJobHandle.Complete();
        PlayerManager.Instance.Players = null;
        networkDriver.Disconnect(connection);
    }

    /// <summary>
    /// Send Requests
    /// </summary>
    public void SendRequest(MessageHeader MessageRequest)
    {
        networkJobHandle.Complete();
        NetworkManager.SendMessage(networkDriver, MessageRequest, connection);
        UIManager.Instance.SideMenu.SlideMenu();
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
    }
}
