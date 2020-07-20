
using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>
{
    //Server
    public List<Players> Players = new List<Players>();
    //Client
    public Players CurrentPlayer;

    /// <summary>
    /// Add playerLabel to the Lobby
    /// </summary>
    public void NewPlayer(MessageHeader packet)
    {
        var message = (NewPlayerMessage)packet;
        Players player = new Players(message.PlayerID, message.PlayerName, message.PlayerColor);
        Players.Add(player);
        UIManager.Instance.SpawnPlayerLabel(player);
    }
}