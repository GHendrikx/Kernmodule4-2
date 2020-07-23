
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
        Debug.Log(message.PlayerID);
        Players player = new Players(message.PlayerID, message.PlayerName, message.PlayerColor);
        Players.Add(player);
        SpawnSprite(player);
        UIManager.Instance.SpawnPlayerLabel(player);
    }

    public void SpawnSprite(MessageHeader Info)
    {
        NewPlayerMessage message = Info as NewPlayerMessage;
        playerCount++;
        GameObject go = GameObject.Instantiate(spritePrefab);
        go.transform.parent = UIManager.Instance.GamePanel.transform;
        go.transform.position = spawnPositions[CurrentPlayer.playerID].transform.position;

        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Debug.Log(message.PlayerColor);
                Color playerColor = new Color();
                playerColor.FromUInt(message.PlayerColor);
                Debug.Log(playerColor);
                Image image = go.transform.GetChild(i).GetComponent<Image>();
                image.color = new Color().FromUInt(message.PlayerColor);
            }

            if (go.transform.GetChild(i).name.Contains("Shield"))
            {
                CurrentPlayer.Shield = go.transform.GetChild(i).gameObject;
                CurrentPlayer.Shield.SetActive(false);
            }
        }
    }

    public void MovePlayer(MessageHeader message)
    {
        MoveRequest moveRequest = message as MoveRequest;
        switch (moveRequest.direction)
        {
            case Direction.North:
                CurrentPlayer.TilePosition.y += 1;
                break;
            case Direction.East:
                CurrentPlayer.TilePosition.x += 1;
                break;
            case Direction.South:
                CurrentPlayer.TilePosition.y -= 1;
                break;
            case Direction.West:
                CurrentPlayer.TilePosition.x -= 1;
                break;
            default:
                break;
        }
    }

    public void SpawnSprite(Players player)
    {

        GameObject go = GameObject.Instantiate(spritePrefab);
        go.transform.parent = UIManager.Instance.GamePanel.transform;

        go.transform.position = spawnPositions[player.playerID].transform.position;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Color playerColor = new Color().FromUInt(player.clientColor);
                go.transform.GetChild(i).GetComponent<Image>().color = playerColor;
            }
            if(go.transform.GetChild(i).name.Contains("Shield"))
            {
                player.Shield = go.transform.GetChild(i).gameObject;
                player.Shield.SetActive(false);
            }
        }
    }

    public void ToggleShield()
    {

    }
}