using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timers;
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
    private GameObject ConnectionPanel;
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
    public Dictionary<int,GameObject> labels = new Dictionary<int,GameObject>();
    [SerializeField]
    private Text deniedText;
    [SerializeField]
    private GameObject attackAnimation;
    [SerializeField]
    private Text MoneyText;
    [SerializeField]
    private GameObject LeaveButton;
    [SerializeField]
    private GameObject disconnectedPanel;
    private int maxPlayers = 0;

    private void Update()
    {
        if (playButton.gameObject.activeInHierarchy)
        {
            if (Content.transform.childCount > maxPlayers)
                playButton.interactable = true;
            else
                playButton.interactable = false;
        }
    }

    public void ShowErrorCode(MessageHeader errorCode)
    {
        RequestDeniedMessage deniedMessage = errorCode as RequestDeniedMessage;
        uint number = deniedMessage.requestDenied;
        string explain = DeniedMessageRules.DeniedMessages[deniedMessage.requestDenied];
        deniedText.text = explain;
        ToggleDeniedGameObject();
        SideMenu.SlideMenu();

        switch ((int)number)
        {
            case 0:
                SwitchToConnectionPanel();
                break;
            case 1:
                SwitchToConnectionPanel();
                break;
        }

        TimerManager.Instance.AddTimer(ToggleDeniedGameObject, 5);
    }

    public void RemovePlayer(MessageHeader arg0)
    {
        PlayerLeftMessage PlayerLeft = arg0 as PlayerLeftMessage;
        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++)
        {
            if(PlayerManager.Instance.Players[i].playerID == PlayerLeft.playerLeftID)
            {
                Players playerToRemove = PlayerManager.Instance.Players[i];
                Destroy(PlayerManager.Instance.Players[i].Sprite.gameObject);
                PlayerManager.Instance.Players.RemoveAt(i);
                break;
            }
        }
    }

    public void ObtainTreasure(MessageHeader arg0)
    {
        ObtainTreasureMessage obtain = arg0 as ObtainTreasureMessage;
        PlayerManager.Instance.CurrentPlayer.treasureAmount += obtain.Amount;
        MoneyText.text = ("$" + PlayerManager.Instance.CurrentPlayer.treasureAmount).ToString();
    }

    public void PlayerDefend(MessageHeader defend)
    {
        PlayerDefendsMessage playerDefend = defend as PlayerDefendsMessage;
        Debug.Log("HI");
        PlayerManager.Instance.Players[playerDefend.PlayerID].DefendOneTurn = true;
        PlayerManager.Instance.Players[playerDefend.PlayerID].Shield.SetActive(true);

    }

    public void ToggleAttackAnimation(MessageHeader arg)
    {
        attackAnimation.gameObject.SetActive(true);
        TimerManager.Instance.AddTimer(DisableAttackAnimation, .3f);
    }

    public void DisableAttackAnimation()
    {
        Debug.Log("Disable");
        attackAnimation.gameObject.SetActive(false);
    }

    private void ToggleDeniedGameObject()
    {
        deniedText.gameObject.SetActive(!deniedText.gameObject.activeInHierarchy);
    }

    /// <summary>
    /// clean the labels of the Lobby. i don't want the labels have a turn
    /// </summary>
    /// <param name="arg0"></param>
    public void DeletePlayerLabels(MessageHeader arg0)
    {
        List<Players> players = PlayerManager.Instance.Players;

        for (int i = 0; i < players.Count; i++)
        {
            // labels doesn;t have the shields thats why i check it on null here.
            if (players[i].Shield == null)
                players.RemoveAt(i);
        }

        if (!players.Contains(PlayerManager.Instance.CurrentPlayer))
            players.Add(PlayerManager.Instance.CurrentPlayer);

    }

    public void DeleteLabels()
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

        Debug.Log("HI");
        for (int i = 0; i < labels.Count; i++)
        {
            Text textComponent = labels[i].GetComponentInChildren<Text>();
            if (textComponent.text.Contains(playerLeftMessage.playerLeftID.ToString()))
            {
                GameObject.Destroy(labels[i].gameObject);
                PlayerManager.Instance.Players.RemoveAt(i);
            }
        }
    }


    /// <summary>
    /// Spawn the label in the lobby
    /// </summary>
    public void SpawnPlayerLabel(Players player)
    {
        GameObject go = GameObject.Instantiate(PlayerLabel);
        go.transform.SetParent( Content.transform,false);
        go.GetComponentInChildren<Text>().text = player.clientName;
        labels.Add(player.playerID,go);
        Color playerColor = new Color();
        RectTransform rectTransform = go.GetComponent<RectTransform>();

        //I need to do this because UNITY, taking the risk
        rectTransform.localPosition = new Vector3(rectTransform.position.x,rectTransform.position.y,0);

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Image image = go.transform.GetChild(i).GetComponent<Image>();
            if (image != null)
                image.color = playerColor.FromUInt(player.clientColor);
        }
    }

    public void SortingPlayerLabel(MessageHeader info)
    {
        foreach (var item in labels.OrderBy(i => i.Key))
            item.Value.transform.SetAsLastSibling();
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

        if (directions.HasFlag((Enum)Direction.West) && directions.HasFlag((Enum)Direction.South) && !directions.HasFlag((Enum)Direction.North) && !directions.HasFlag((Enum)Direction.East))
            LeaveButton.SetActive(true);
        else
            LeaveButton.SetActive(false);

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

    public void DisconnectedPanel()
    {
    }

    public void ToggleLeaveButton(bool toggle)
    {
        LeaveButton.SetActive(toggle);
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

    public void SwitchToConnectionPanel()
    {
        gamePanel.SetActive(false);
        lobbyPanel.SetActive(false);
        ConnectionPanel.SetActive(true);
    }

    public void AttackMonster(int i)
    {
        Vector2 currentPosition = PlayerManager.Instance.Players[i].TilePosition;

        Tile tile = GameManager.Instance.CurrentGrid.tilesArray[(int)currentPosition.x, (int)currentPosition.y];
        tile.MonsterHealth = 0;

        if (tile.Content == TileContent.Monster)
            tile.Content = TileContent.None;
        else if(tile.Content == TileContent.Both)
            tile.Content = TileContent.Treasure;

        monsterSprite.SetActive(false);
    }
}
