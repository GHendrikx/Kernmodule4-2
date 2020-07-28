using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private GameObject PlayerLabel;
    [SerializeField]
    private GameObject Content;
    [SerializeField]
    private Button playButton;
    public Button PlayButton
    {
        get
        {
            return playButton;
        }
        set
        {
            playButton = value;
        }
    }
    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private GameObject gamePanel;
    public GameObject GamePanel
    {
        get
        {
            return gamePanel;
        }
        set
        {
            gamePanel = value;
        }
    }
    [SerializeField]
    private SideMenu sideMenu;
    public SideMenu SideMenu
    {
        get
        {
            return sideMenu;
        }
        set
        {
            sideMenu = value;
        }
    }

    public GameObject[] doors;
    public GameObject monsterSprite;
    public GameObject treasureSprite;
    public List<GameObject> labels = new List<GameObject>();
    public Text debugText;
    private void Update()
    {
        if (playButton.gameObject.activeInHierarchy)
        {
            if (Content.transform.childCount > 0)
                playButton.interactable = true;
            else
                playButton.interactable = false;
        }

    }

    public void DeleteTrash(MessageHeader arg0)
    {
        List<Players> players = PlayerManager.Instance.Players;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Shield == null)
                players.RemoveAt(i);
        }
        if (!players.Contains(PlayerManager.Instance.CurrentPlayer))
            players.Add(PlayerManager.Instance.CurrentPlayer);

    }

    public void DeleteTrash()
    {
        List<Players> players = PlayerManager.Instance.Players;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].Shield == null)
                players.RemoveAt(i);
        }
        if (!players.Contains(PlayerManager.Instance.CurrentPlayer))
            players.Add(PlayerManager.Instance.CurrentPlayer);
    }

    public void EnterPlayer(MessageHeader playerEnter)
    {
        PlayerEnterRoomMessage enterRoom = playerEnter as PlayerEnterRoomMessage;
        Players enterRoomPlayer = PlayerManager.Instance.Players[enterRoom.PlayerID];
        Debug.Log("SpriteEnterRoom " + enterRoomPlayer);
        enterRoomPlayer.Sprite.SetActive(true);
    }
    public void LeavePlayer(MessageHeader playerLeave)
    {
        PlayerLeaveRoomMessage leaveRoom = playerLeave as PlayerLeaveRoomMessage;
        Players player = PlayerManager.Instance.Players[leaveRoom.PlayerID];
        player.Sprite.gameObject.SetActive(false);
    }

    public void DisableSpawnLabel(MessageHeader message)
    {
        PlayerLeftMessage playerLeftMessage = message as PlayerLeftMessage;
        for (int i = 0; i < labels.Count; i++)
        {
            Text textComponent = labels[i].GetComponentInChildren<Text>();
            if (textComponent.text.Contains(playerLeftMessage.playerLeftID.ToString()))
            {
                GameObject.Destroy(labels[i].gameObject);
                PlayerManager.Instance.Players[i].Sprite.SetActive(false);
                PlayerManager.Instance.Players[i] = null;
            }
        }
    }


    /// <summary>
    /// Spawn the label in the lobby
    /// </summary>
    public void SpawnPlayerLabel(Players player)
    {
        GameObject go = GameObject.Instantiate(PlayerLabel);
        go.transform.parent = Content.transform;
        go.GetComponentInChildren<Text>().text = player.playerID + " " + player.clientName;
        labels.Add(go);
        Color playerColor = new Color();

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Image image = go.transform.GetChild(i).GetComponent<Image>();
            if (image != null)
                image.color = playerColor.FromUInt(player.clientColor);
        }

    }

    public void ShowNewRoom(MessageHeader roomInfo)
    {
        RoomInfoMessage info = roomInfo as RoomInfoMessage;
        Direction directions = (Direction)info.directions;

        if (directions.HasFlag((Enum)Direction.North))
            doors[0].SetActive(true);
        else
            doors[0].SetActive(false);

        if (directions.HasFlag((Enum)Direction.East))
            doors[1].SetActive(true);
        else
            doors[1].SetActive(false);

        if (directions.HasFlag((Enum)Direction.South))
            doors[2].SetActive(true);
        else
            doors[3].SetActive(false);

        if (directions.HasFlag((Enum)Direction.West))
            doors[3].SetActive(true);
        else
            doors[3].SetActive(false);

        if (info.ContainsMonster == 1)
            monsterSprite.gameObject.SetActive(true);
        else
            monsterSprite.gameObject.SetActive(false);

        if (info.TreasureInRoom == 1)
            treasureSprite.gameObject.SetActive(true);
        else
            treasureSprite.gameObject.SetActive(false);

        PlayerManager.Instance.CurrentPlayer.Sprite.SetActive(true);

        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++)
        {
            if (info.OtherPlayerIDs.Contains(PlayerManager.Instance.Players[i].playerID))
                PlayerManager.Instance.Players[i].Sprite.SetActive(true);
            else
                PlayerManager.Instance.Players[i].Sprite.SetActive(false);

        }
    }

    public void CheckTurn(MessageHeader playerTurnMessage)
    {
        PlayerTurnMessage turnMessage = playerTurnMessage as PlayerTurnMessage;
        PlayerManager.Instance.PlayerIDWithTurn = turnMessage.playerID;

        //Means its the players Turn 
        if (turnMessage.playerID == PlayerManager.Instance.CurrentPlayer.playerID)
            sideMenu.SlideMenu();
    }

    public void SwitchToGamePanel(MessageHeader packet)
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);
    }

    public void AttackMonster(int i)
    {
        Vector2 currentPosition = PlayerManager.Instance.Players[i].TilePosition;
        Tile tile = GameManager.Instance.currentGrid.tilesArray[(int)currentPosition.x, (int)currentPosition.y];
        tile.MonsterHealth = 0;

        if (tile.Content == TileContent.Monster)
            tile.Content = TileContent.None;
        else
            tile.Content = TileContent.Treasure;
    }
}
