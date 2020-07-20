﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using Assets.Code;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;

public class ServerBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];
  
    private Queue<MessageHeader> serverMessagesQueue;
    public Queue<MessageHeader> ServerMessageQueue
    {
        get
        {
            return serverMessagesQueue;
        }
    }

    void Start()
    {
        networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if(networkDriver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind port");
        else
            networkDriver.Listen();

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        serverMessagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ServerCallbacks.Length; i++)
            ServerCallbacks[i] = new MessageEvent();

        ServerCallbacks[(int)MessageHeader.MessageType.SetName].AddListener(HandleSetName);

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
        if(connections.Length < 5)
            while((c = networkDriver.Accept()) != default)
            {
                //Accepted Connection
                connections.Add(c);
                var colour = (Color32)Random.ColorHSV();
                var welcomeMessage = new WelcomeMessage
                {
                    PlayerID = c.InternalId,
                    Colour = ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a
                };

                PlayerManager.Instance.Players.Add(new Players(welcomeMessage.PlayerID, "", welcomeMessage.Colour));
                NetworkManager.SendMessage(networkDriver, welcomeMessage, c);
            }

        DataStreamReader reader;
        for(int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if(cmd == NetworkEvent.Type.Data)
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
                                if (connections[j].InternalId != newPlayerMessage.PlayerID)
                                {
                                    NetworkManager.SendMessage(networkDriver, newPlayerMessage, connections[j]);
                                    var currentPlayerMessage = new NewPlayerMessage()
                                    {
                                        PlayerID = PlayerManager.Instance.Players[j].playerID,
                                        PlayerColor = PlayerManager.Instance.Players[j].clientColor,
                                        PlayerName = PlayerManager.Instance.Players[j].clientName
                                    };
                                    NetworkManager.SendMessage(networkDriver, currentPlayerMessage, connections[i]);
                                }

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
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void ProcessMessagesQueue()
    {
        while(serverMessagesQueue.Count > 0)
        {
            var message = serverMessagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }
    }

    public void StartGame()
    {
        StartGameMessage message = new StartGameMessage();
        for (int i = 0; i < connections.Length; i++)
            NetworkManager.SendMessage(networkDriver, message, connections[i]);
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }
}
