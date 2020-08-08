using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Assets.Code;
using Unity.Jobs;
using System;
using UnityEngine.Timers;

public class ServerBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;
    private PlayerTurnMessage turnMessage;
    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count];

    public bool GameHasStarted = false;

    private Queue<MessageHeader> serverMessagesQueue;
    public Queue<MessageHeader> ServerMessageQueue
    {
        get
        {
            return serverMessagesQueue;
        }
    }
    public int playerID = 0;
    public string IPAdress;
    void Start()
    {
        networkDriver = NetworkDriver.Create();

        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;

        if (!GameManager.Instance.LOCAL)
            endpoint = NetworkEndPoint.Parse(IPAdress, 9000);

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
        foreach (NetworkConnection c in connections)
            NetworkManager.SendMessage(networkDriver, arg0, c);
    }

    private void HandleSetName(MessageHeader message)
    {
        //Debug.Log($"Got a name: {(message as SetNameMessage).Name}");
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
        while ((c = networkDriver.Accept()) != default)
        {
            if (GameHasStarted)
            {
                MakeRequestDeniedMessage(c, 0);
                c.Disconnect(networkDriver);
                return;
            }

            if (connections.Length > 4)
            {
                MakeRequestDeniedMessage(c, 1);
                c.Disconnect(networkDriver);
                return;
            }

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

                        case MessageHeader.MessageType.PlayerLeft:
                            PlayerLeftMessage leftMessage = NetworkManager.ReadMessage<PlayerLeftMessage>(reader, ServerMessageQueue) as PlayerLeftMessage;
                            leftMessage.playerLeftID = (uint)i;
                            
                            for (int j = 0; j < connections.Length; j++)
                                if (leftMessage.playerLeftID != PlayerManager.Instance.Players[i].playerID)
                                    NetworkManager.SendMessage(networkDriver, leftMessage, connections[i]);
                            connections[i] = default;
                            break;

                        case MessageHeader.MessageType.MoveRequest:
                            var moveRequest = NetworkManager.ReadMessage<MoveRequest>(reader, ServerMessageQueue);
                            Debug.Log("MoveRequest");
                            if (CheckTileContent(i, TileContent.Monster) || CheckTileContent(i, TileContent.Both))
                            {
                                MakeRequestDeniedMessage(i, 2);
                                SendNewRoomInfo();
                                return;
                            }

                            MakeLeaveRoomMessage(i);
                            PlayerManager.Instance.MovePlayer(moveRequest, i);
                            MakeEnterRoommessage(i);

                            SendNewRoomInfo();
                            MonsterTurn();
                            NewTurnMessage();
                            break;

                        case MessageHeader.MessageType.AttackRequest:
                            var request = NetworkManager.ReadMessage<AttackRequestMessage>(reader, serverMessagesQueue);
                            Debug.Log(DebugTileContent(i));

                            if (!CheckTileContent(i, TileContent.Monster) && !CheckTileContent(i, TileContent.Both))
                            {
                                Debug.Log(CheckTileContent(i, TileContent.Monster));
                                Debug.Log(CheckTileContent(i, TileContent.Both));

                                MakeRequestDeniedMessage(i, 4);
                                SendNewRoomInfo();
                                return;
                            }

                            if (CheckTileContent(i, TileContent.Monster))
                                SetTileContent(i, TileContent.None);
                            if (CheckTileContent(i, TileContent.Both))
                                SetTileContent(i, TileContent.Treasure);


                            var message = new HitMonsterMessage();
                            NetworkManager.SendMessage(networkDriver, message, connections[i]);
                            SendNewRoomInfo();
                            NewTurnMessage();
                            MonsterTurn();
                            break;

                        case MessageHeader.MessageType.DefendRequest:
                            NetworkManager.ReadMessage<DefendRequestMessage>(reader, serverMessagesQueue);
                            PlayerManager.Instance.Players[i].DefendOneTurn = true;
                            var playerDefendMessage = new PlayerDefendsMessage()
                            {
                                PlayerID = PlayerManager.Instance.Players[i].playerID
                            };

                            Debug.Log(connections.Length);
                            for (int j = 0; j < connections.Length; j++)
                                NetworkManager.SendMessage(networkDriver, playerDefendMessage, connections[j]);

                            SendNewRoomInfo();
                            NewTurnMessage();
                            MonsterTurn();
                            break;

                        case MessageHeader.MessageType.ObtainTreasureRequest:
                            NetworkManager.ReadMessage<ClaimTreasureRequestMessage>(reader, serverMessagesQueue);

                            if (!CheckTileContent(i, TileContent.Treasure) && !CheckTileContent(i, TileContent.Both))
                            {
                                MakeRequestDeniedMessage(i, 5);
                                SendNewRoomInfo();
                                return;
                            }
                            if (CheckTileContent(i, TileContent.Both))
                            {
                                MakeRequestDeniedMessage(i, 6);
                                SendNewRoomInfo();
                                return;
                            }

                            Dictionary<Players, uint> players = PlayerManager.Instance.ClaimTreasureDivideItForPlayers(i);

                            uint amount;

                            int SendedItem = 0;

                            foreach (KeyValuePair<Players,uint> item in players)
                            {
                                amount = item.Value;
                                ObtainTreasureMessage obtainTreasure = new ObtainTreasureMessage()
                                {
                                    Amount = (ushort)amount
                                };

                                for (int j = 0; j < PlayerManager.Instance.Players.Count; j++)
                                    if (PlayerManager.Instance.Players[j].playerID == item.Key.playerID)
                                    {
                                        NetworkManager.SendMessage(networkDriver, obtainTreasure, connections[j]);
                                        SendedItem++;
                                    }
                            }

                            SetTileContent(i, TileContent.None);
                            SendNewRoomInfo();
                            MonsterTurn();
                            NewTurnMessage();
                            break;


                        case MessageHeader.MessageType.LeaveDungeonRequest:
                            NetworkManager.ReadMessage<LeaveDungeonRequest>(reader, serverMessagesQueue);
                            PlayerManager.Instance.PlayersWhoLeft.Add(PlayerManager.Instance.Players[i]);
                            
                            PlayerLeftDungeonMessage playerLeftDungeonMessage = new PlayerLeftDungeonMessage()
                            {
                                playerID = i
                            };

                            PlayerManager.Instance.Players.RemoveAt(i);

                            for (int j = 0; j < connections.Length; j++)
                            {
                                if (i != j)
                                    NetworkManager.SendMessage(networkDriver, playerLeftDungeonMessage, connections[j]);
                            }

                            if (PlayerManager.Instance.Players.Count < 1)
                            {
                                EndGameMessage endGameMessage = new EndGameMessage();
                                for (int j = 0; j < PlayerManager.Instance.PlayersWhoLeft.Count; j++)
                                {
                                    endGameMessage.playerID.Add(PlayerManager.Instance.PlayersWhoLeft[j].playerID);
                                    endGameMessage.highScorePairs.Add((ushort)PlayerManager.Instance.PlayersWhoLeft[j].treasureAmount);
                                    NetworkManager.SendMessage(networkDriver, endGameMessage, connections[j]);
                                    connections[j].Disconnect(networkDriver);
                                }
                            }
                            else
                            {

                                MonsterTurn();
                                SendNewRoomInfo();
                                NewTurnMessage();
                            }
                            break;

                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {

                    PlayerLeftMessage leftMessage = new PlayerLeftMessage();
                    leftMessage.playerLeftID = (uint)i;

                    for (int j = 0; j < connections.Length; j++)
                        if (leftMessage.playerLeftID != PlayerManager.Instance.Players[j].playerID)
                            NetworkManager.SendMessage(networkDriver, leftMessage, connections[j]);

                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private Players SendGetPlayersInTheSameRoom(int i)
    {
        Players currentPlayer = PlayerManager.Instance.Players[i];
        return currentPlayer;

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

    private bool CheckTileContent(int i, TileContent tileContent)
    {
        Vector2 tilePosition = PlayerManager.Instance.Players[i].TilePosition;
        if (GameManager.Instance.CurrentGrid.tilesArray[(int)tilePosition.x, (int)tilePosition.y].Content == tileContent)
        {
            return true;
        }
        return false;
    }

    private TileContent DebugTileContent(int i)
    {
        Vector2 tilePosition = PlayerManager.Instance.Players[i].TilePosition;

        return GameManager.Instance.CurrentGrid.tilesArray[(int)tilePosition.x, (int)tilePosition.y].Content;
    }

    private void SetTileContent(int i, TileContent tileContent)
    {
        Vector2 tilePosition = PlayerManager.Instance.Players[i].TilePosition;
        GameManager.Instance.CurrentGrid.tilesArray[(int)tilePosition.x, (int)tilePosition.y].Content = tileContent;
    }

    private void MakeRequestDeniedMessage(int i, int number)
    {
        var requestDenied = new RequestDeniedMessage()
        {
            requestDenied = (uint)number
        };

        NetworkManager.SendMessage(networkDriver, requestDenied, connections[i]);

    }

    private void MakeRequestDeniedMessage(NetworkConnection connection, int number)
    {
        var requestDenied = new RequestDeniedMessage()
        {
            requestDenied = (uint)number
        };
        NetworkManager.SendMessage(networkDriver, requestDenied, connection);
    }

    private void MakeLeaveRoomMessage(int i)
    {
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

    private void MonsterTurn()
    {
        HitByMonsterMessage hitByMonsterMessage = new HitByMonsterMessage();

        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++)
        {
            Players player = PlayerManager.Instance.Players[i];

            Vector2 playerPosition = player.TilePosition;
            Tile t = GameManager.Instance.CurrentGrid.tilesArray[(int)playerPosition.x, (int)playerPosition.y];

            if ((t.Content == TileContent.Monster) || (t.Content == TileContent.Both))
            {
                if (player.DefendOneTurn)
                {
                    player.DefendOneTurn = false;
                    NetworkManager.SendMessage(networkDriver, hitByMonsterMessage, connections[i]);
                    continue;
                }


                NetworkManager.SendMessage(networkDriver, hitByMonsterMessage, connections[i]);

                    player.Health -= 1;
                    if (player.Health < 1)
                    {
                        PlayerDiesMessage playerDiesMessage = new PlayerDiesMessage();
                        NetworkManager.SendMessage(networkDriver, playerDiesMessage, connections[i]);
                    }
            }
        }

    }

    private void SendNewRoomInfo()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            RoomInfoMessage info = GameManager.Instance.MakeRoomInfoMessage(i);
            NetworkManager.SendMessage(networkDriver, info, connections[i]);
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

        GameHasStarted = true;
        GameObject go = new GameObject();
        Grid grid = go.AddComponent<Grid>();
        grid.gameObject.name = "Grid";
        grid.GenerateGrid();
        GameManager.Instance.CurrentGrid = grid;

        for (int i = 0; i < connections.Length; i++)
            NetworkManager.SendMessage(networkDriver, startGameMessage, connections[i]);
        PlayerManager.Instance.SortingPlayerList();
        PlayerTurnMessage playerTurnMessage = new PlayerTurnMessage()
        {
            playerID = 0
        };

        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++)
            PlayerManager.Instance.Players[i].Health = 10;

        for (int i = 0; i < connections.Length; i++)
            NetworkManager.SendMessage(networkDriver, playerTurnMessage, connections[i]);
        UIManager.Instance.DeleteLabels();
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }
}
