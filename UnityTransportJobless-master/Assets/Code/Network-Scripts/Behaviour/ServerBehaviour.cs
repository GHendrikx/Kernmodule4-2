using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Assets.Code;
using Unity.Jobs;

public class ServerBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;
    private PlayerTurnMessage turnMessage;
    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count];

    private Queue<MessageHeader> serverMessagesQueue;
    public Queue<MessageHeader> ServerMessageQueue
    {
        get
        {
            return serverMessagesQueue;
        }
    }
    public int playerID = 0;
    void Start()
    {
        networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (networkDriver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind port");
        else
            networkDriver.Listen();

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        serverMessagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ServerCallbacks.Length; i++)
            ServerCallbacks[i] = new MessageEvent();
        ServerCallbacks[(int)MessageHeader.MessageType.SetName].AddListener(HandleSetName);
        ServerCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(DisconnectClient);
    }

    private void DisconnectClient(MessageHeader arg0)
    {
        foreach(NetworkConnection c in connections)
            NetworkManager.SendMessage(networkDriver, arg0, c);
    }

    private void HandleSetName(MessageHeader message)
    {
        Debug.Log($"Got a name: {(message as SetNameMessage).Name}");
    }

    void Update()
    {
        networkJobHandle.Complete();
        for (int i = 0; i < connections.Length; ++i)
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }

        NetworkConnection c;
        if (connections.Length < 5)
            while ((c = networkDriver.Accept()) != default)
            {
                //Accepted Connection
                connections.Add(c);
               
                var colour = (Color32)UnityEngine.Random.ColorHSV();
                var welcomeMessage = new WelcomeMessage
                {
                    PlayerID = playerID,
                    Colour = ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a
                };

                PlayerManager.Instance.Players.Add(new Players(playerID, "", welcomeMessage.Colour));
                playerID++;
                NetworkManager.SendMessage(networkDriver, welcomeMessage, c);
            }

        DataStreamReader reader;
        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                    switch (messageType)
                    {
                        case MessageHeader.MessageType.None:
                            var noneMessage = NetworkManager.ReadMessage<StayAliveMessage>(reader, ServerMessageQueue);
                            NetworkManager.SendMessage(networkDriver, noneMessage, connections[i]);
                            break;
                        case MessageHeader.MessageType.NewPlayer:
                            break;
                        case MessageHeader.MessageType.Welcome:
                            break;
                        case MessageHeader.MessageType.SetName:
                            SetNameMessage setNameMessage = NetworkManager.ReadMessage<SetNameMessage>(reader, ServerMessageQueue) as SetNameMessage;
                            PlayerManager.Instance.Players[i].clientName = setNameMessage.Name;

                            var newPlayerMessage = new NewPlayerMessage()
                            {
                                PlayerID = PlayerManager.Instance.Players[i].playerID,
                                PlayerColor = PlayerManager.Instance.Players[i].clientColor,
                                PlayerName = setNameMessage.Name
                            };

                            //looping through all the connections to send the new player message
                            for (int j = 0; j < connections.Length; j++)
                                if (j != i)
                                {
                                    NetworkManager.SendMessage(networkDriver, newPlayerMessage, connections[j]);

                                    var connectedPlayersMessage = new NewPlayerMessage()
                                    {
                                        PlayerID = PlayerManager.Instance.Players[j].playerID,
                                        PlayerColor = PlayerManager.Instance.Players[j].clientColor,
                                        PlayerName = PlayerManager.Instance.Players[j].clientName
                                    };

                                    NetworkManager.SendMessage(networkDriver, connectedPlayersMessage, connections[i]);
                                }

                            break;
                        case MessageHeader.MessageType.RequestDenied:
                            break;
                        case MessageHeader.MessageType.PlayerLeft:
                            break;
                        case MessageHeader.MessageType.PlayerTurn:
                            break;
                        case MessageHeader.MessageType.RoomInfo:
                            break;
                        case MessageHeader.MessageType.PlayerEnterRoom:
                            break;
                        case MessageHeader.MessageType.PlayerLeaveRoom:
                            break;
                        case MessageHeader.MessageType.ObtainTreasure:
                            break;
                        case MessageHeader.MessageType.HitMonster:
                            break;
                        case MessageHeader.MessageType.HitByMonster:
                            break;
                        case MessageHeader.MessageType.PlayerDefends:
                            break;
                        case MessageHeader.MessageType.PlayerLeftDungeon:
                            break;
                        case MessageHeader.MessageType.PlayerDies:
                            break;
                        case MessageHeader.MessageType.EndGame:
                            break;
                        case MessageHeader.MessageType.MoveRequest:
                            var moveRequest = NetworkManager.ReadMessage<MoveRequest>(reader, ServerMessageQueue);
                            MakeLeaveRoomMessage(i);
                            PlayerManager.Instance.MovePlayer(moveRequest, i);
                            MakeEnterRoommessage(i);
                            Debug.Log(PlayerManager.Instance.Players[i].TilePosition);
                            NewTurnMessage();
                            break;

                        case MessageHeader.MessageType.AttackRequest:
                            UIManager.Instance.AttackMonster(i);
                            SendNewRoomInfo();
                            NewTurnMessage();
                            break;
                        case MessageHeader.MessageType.DefendRequest:
                            NetworkManager.ReadMessage<DefendRequestMessage>(reader, serverMessagesQueue);
                            SendNewRoomInfo();
                            NewTurnMessage();
                            break;

                        case MessageHeader.MessageType.ClaimTreasureRequest:
                            NetworkManager.ReadMessage<ObtainTreasureMessage>(reader, serverMessagesQueue);
                            PlayerManager.Instance.ClaimTreasure(i);
                            SendNewRoomInfo();
                            NewTurnMessage();
                            break;

                        case MessageHeader.MessageType.LeaveDungeonRequest:
                            NetworkManager.ReadMessage<LeaveDungeonRequest>(reader, serverMessagesQueue);
                            SendNewRoomInfo();
                            NewTurnMessage();
                            break;

                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void MakeEnterRoommessage(int i)
    {
        for (int j = 0; j < connections.Length; j++)
        {
            Players currentPlayer = PlayerManager.Instance.Players[i];
            Players compairePlayer = PlayerManager.Instance.Players[j];

            if (currentPlayer.TilePosition == compairePlayer.TilePosition)
            {

                var enterRoom = new PlayerEnterRoomMessage()
                {
                    PlayerID = PlayerManager.Instance.Players[i].playerID
                };

                NetworkManager.SendMessage(networkDriver, enterRoom, connections[j]);
            }
        }
        SendNewRoomInfo();
    }

    private void MakeLeaveRoomMessage(int i)
    {
        Debug.Log("leave");

        for (int j = 0; j < connections.Length; j++)
        {
            Players currentPlayer = PlayerManager.Instance.Players[i];
            Players compairePlayer = PlayerManager.Instance.Players[j];

            if (currentPlayer.TilePosition == compairePlayer.TilePosition)
            {
                var LeaveRoom = new PlayerLeaveRoomMessage()
                {
                    PlayerID = PlayerManager.Instance.Players[i].playerID
                };
                NetworkManager.SendMessage(networkDriver, LeaveRoom, connections[j]);
            }
        }
        SendNewRoomInfo();
    }

    private void NewTurnMessage()
    {
        PlayerManager.Instance.PlayerIDWithTurn++;
        if (PlayerManager.Instance.PlayerIDWithTurn == PlayerManager.Instance.Players.Count)
            PlayerManager.Instance.PlayerIDWithTurn = 0;

        turnMessage = new PlayerTurnMessage()
        {
            playerID = PlayerManager.Instance.PlayerIDWithTurn
        };

        for (int j = 0; j < connections.Length; j++)
            NetworkManager.SendMessage(networkDriver, turnMessage, connections[j]);
    }
    private void SendNewRoomInfo()
    {
        for (int j = 0; j < connections.Length; j++)
        {
            RoomInfoMessage info = GameManager.Instance.MakeRoomInfoMessage(j);
            NetworkManager.SendMessage(networkDriver, info, connections[j]);
        }
    }

    private void ProcessMessagesQueue()
    {
        while (serverMessagesQueue.Count > 0)
        {
            var message = serverMessagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }
    }

    public void StartGame()
    {
        networkJobHandle.Complete();
        StartGameMessage startGameMessage = new StartGameMessage()
        {
            StartHP = 10
        };

        GameObject go = new GameObject();
        Grid grid = GameObject.Instantiate(go).AddComponent<Grid>();
        grid.gameObject.name = "Grid";
        grid.GenerateGrid();
        GameManager.Instance.currentGrid = grid;

        for (int i = 0; i < connections.Length; i++)
            NetworkManager.SendMessage(networkDriver, startGameMessage, connections[i]);

        PlayerTurnMessage playerTurnMessage = new PlayerTurnMessage()
        {
            playerID = 0
        };

        for (int i = 0; i < connections.Length; i++)
            NetworkManager.SendMessage(networkDriver, playerTurnMessage, connections[i]);
        UIManager.Instance.DeleteTrash();
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }
}
