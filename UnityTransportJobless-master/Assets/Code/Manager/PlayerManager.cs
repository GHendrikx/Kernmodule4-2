
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
    [SerializeField]
    private GameObject spritePrefab;
    private static int playerCount;
    [SerializeField]
    private GameObject[] spawnPositions;

    /// <summary>
    /// Add playerLabel to the Lobby
    /// </summary>
    public void NewPlayer(MessageHeader packet)
    {
        var message = (NewPlayerMessage)packet;
        Debug.Log("Y");
        Players player = new Players(message.PlayerID, message.PlayerName, message.PlayerColor);
        Players.Add(player);
        UIManager.Instance.SpawnPlayerLabel(player);
    }

    public void SpawnSprite(MessageHeader Info)
    {
        NewPlayerMessage message = Info as NewPlayerMessage;
        playerCount++;
        GameObject go = GameObject.Instantiate(spritePrefab);
        go.transform.parent = UIManager.Instance.GamePanel.transform;

        for (int i = 0; i < go.transform.childCount; i++)
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Debug.Log(message.PlayerColor);
                Color playerColor = new Color();
                playerColor.FromUInt(message.PlayerColor);
                Debug.Log(playerColor);
                Image image = go.transform.GetChild(i).GetComponent<Image>();
                image.color = new Color().FromUInt(message.PlayerColor);
            }
    }

    public void SpawnSprite(Players player)
    {
        playerCount++;
        GameObject go = GameObject.Instantiate(spritePrefab);
        go.transform.parent = UIManager.Instance.GamePanel.transform;
        go.transform.position = spawnPositions[playerCount].transform.position;
        for (int i = 0; i < go.transform.childCount; i++)
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Color playerColor = new Color().FromUInt(player.clientColor);
                go.transform.GetChild(i).GetComponent<Image>().color = playerColor;
            }
    }
}