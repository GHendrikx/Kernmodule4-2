using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>
{
    //Server
    public List<Players> Players = new List<Players>();
    public List<Players> PlayersWhoLeft = new List<Players>();
    //Client
    public Players CurrentPlayer;
    [SerializeField]
    private GameObject spritePrefab;
    public int PlayerIDWithTurn = 0;
    [SerializeField]
    private GameObject playerDiesPanel;
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
    [SerializeField]
    private GameObject gamePanel;
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
        go.transform.SetParent(UIManager.Instance.GamePanel.transform, false);
        go.transform.localScale = Vector3.one;
        go.transform.position = spawnPositions[CurrentPlayer.playerID].transform.position;


        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Debug.Log(message.PlayerColor);
                Color playerColor = new Color();
                playerColor.FromUInt(message.PlayerColor);
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

    public void SetBeginHealth(MessageHeader arg0)
    {
        StartGameMessage startGame = arg0 as StartGameMessage;
        CurrentPlayer.Health = 10;
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

    public void PlayerDies(MessageHeader arg0)
    {
        playerDiesPanel.SetActive(true);
        gamePanel.SetActive(false);
    }

    public void HitByMonster(MessageHeader arg0)
    {
        if (CurrentPlayer.Shield.activeInHierarchy)
            CurrentPlayer.Shield.SetActive(false);
        else
            CurrentPlayer.Health -= 1;
    }

    public void SpawnSprite(Players player)
    {
        GameObject go = GameObject.Instantiate(spritePrefab);
        player.Sprite = go;
        go.transform.SetParent(UIManager.Instance.GamePanel.transform, false);

        go.transform.position = spawnPositions[player.playerID].transform.position;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).name == "Arrow")
            {
                Color playerColor = new Color().FromUInt(player.clientColor);
                go.transform.GetChild(i).GetComponent<Image>().color = playerColor;
                player.Arrow = go.transform.GetChild(i).gameObject;
            }
            if (go.transform.GetChild(i).name.Contains("Shield"))
            {
                player.Shield = go.transform.GetChild(i).gameObject;
                player.Shield.SetActive(false);
            }
        }
    }

    public void SpawnSprite(ref Players player)
    {
        GameObject go = GameObject.Instantiate(spritePrefab);
        player.Sprite = go;
        go.transform.SetParent(UIManager.Instance.GamePanel.transform, false);

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

    public Dictionary<Players, uint> ClaimTreasureDivideItForPlayers(int playerIndex)
    {
        Vector2 playerTile = Players[playerIndex].TilePosition;
        List<Players> whoGotTheSplit = new List<Players>();

        for (int i = 0; i < Players.Count; i++)
            if (playerTile == Players[i].TilePosition)
                whoGotTheSplit.Add(Players[i]);

        int amount = GameManager.Instance.CurrentGrid.tilesArray[(int)playerTile.x, (int)playerTile.y].TreasureAmount / whoGotTheSplit.Count;
        
        Dictionary<Players, uint> price = new Dictionary<Players, uint>();

        for (int i = 0; i < whoGotTheSplit.Count; i++)
            price.Add(whoGotTheSplit[i], (uint)amount);
        
        GameManager.Instance.CurrentGrid.tilesArray[(int)playerTile.x, (int)playerTile.y].TreasureAmount = 0;

        return price;

    }

    /// <summary>
    /// Bubble Sorting the playerList
    /// </summary>
    public void SortingPlayerList()
    {
        var moveItem = false;
        do
        {
            moveItem = false;
            for (int i = 0; i < Players.Count - 1; i++)
            {
                if (Players[i].playerID > Players[i + 1].playerID)
                {
                    var lowerValue = Players[i + 1];
                    Players[i + 1] = Players[i];
                    Players[i] = lowerValue;
                    moveItem = true;
                }
            }
        } while (moveItem);
    }
}