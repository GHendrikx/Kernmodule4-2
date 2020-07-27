
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
    public int PlayerIDWithTurn = 0;
    [SerializeField]
    private GameObject[] spawnPositions;
    public GameObject[] SpawnPositions
    {
        get
        {
            return spawnPositions;
        }
        set
        {
            spawnPositions = value;
        }
    }

    /// <summary>
    /// Add playerLabel to the Lobby
    /// </summary>
    public void NewPlayer(MessageHeader packet)
    {
        var message = (NewPlayerMessage)packet;
        Players player = new Players(message.PlayerID, message.PlayerName, message.PlayerColor);
        SpawnSprite(ref player);
        UIManager.Instance.SpawnPlayerLabel(player);
        Players.Add(player);
    }

    public void SpawnSprite(MessageHeader Info)
    {
        NewPlayerMessage message = Info as NewPlayerMessage;
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

    public void MovePlayer(MessageHeader message, int playerIndex)
    {
        MoveRequest moveRequest = message as MoveRequest;

        switch (moveRequest.direction)
        {
            case Direction.North:
                Players[playerIndex].TilePosition.y += 1;
                break;
            case Direction.East:
                Players[playerIndex].TilePosition.x += 1;
                break;
            case Direction.South:
                Players[playerIndex].TilePosition.y -= 1;
                break;
            case Direction.West:
                Players[playerIndex].TilePosition.x -= 1;
                break;

        }
    }

    public void SpawnSprite(Players player)
    {
        Debug.Log("HI");
        GameObject go = GameObject.Instantiate(spritePrefab);
        player.Sprite = go;
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

    public void SpawnSprite(ref Players player)
    {
        Debug.Log("HI");
        GameObject go = GameObject.Instantiate(spritePrefab);
        player.Sprite = go;
        go.transform.parent = UIManager.Instance.GamePanel.transform;

        go.transform.position = spawnPositions[player.playerID].transform.position;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Color playerColor = new Color().FromUInt(player.clientColor);
                go.transform.GetChild(i).GetComponent<Image>().color = playerColor;
            }
            if (go.transform.GetChild(i).name.Contains("Shield"))
            {
                player.Shield = go.transform.GetChild(i).gameObject;
                player.Shield.SetActive(false);
            }
        }
    }

    public void ClaimTreasure(int i)
    {
        Vector2 playerTile = PlayerManager.Instance.Players[i].TilePosition;
        uint amount = (uint)GameManager.Instance.currentGrid.tilesArray[(int)playerTile.x, (int)playerTile.y].RandomTreasureAmount;
        PlayerManager.Instance.Players[i].treasureAmount = amount;
    }
}