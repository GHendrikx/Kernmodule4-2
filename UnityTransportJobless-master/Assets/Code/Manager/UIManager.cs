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

    public GameObject[] doors;
    public GameObject monster;
    public GameObject treasure;

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

    /// <summary>
    /// Spawn the label in the lobby
    /// </summary>
    public void SpawnPlayerLabel(Players player)
    {
        GameObject go = GameObject.Instantiate(PlayerLabel);
        go.transform.parent = Content.transform;
        go.GetComponentInChildren<Text>().text = player.clientName;

        Color playerColor = new Color();

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Image image = go.transform.GetChild(i).GetComponent<Image>();
            if(image != null)
                image.color = playerColor.FromUInt(player.clientColor);
        }
        
    }

    public void ShowNewRoom(MessageHeader roomInfo)
    {
        RoomInfoMessage info = roomInfo as RoomInfoMessage;
        Direction directions = (Direction)info.directions;

        if (directions.HasFlag((Enum)Direction.North))
            doors[0].SetActive(true);
        if (directions.HasFlag((Enum)Direction.East))
            doors[1].SetActive(true);
        if (directions.HasFlag((Enum)Direction.South))
            doors[2].SetActive(true);
        if (directions.HasFlag((Enum)Direction.West))
            doors[3].SetActive(true);
    }

    public void CheckTurn(MessageHeader playerTurnMessage)
    {
        PlayerTurnMessage turnMessage = playerTurnMessage as PlayerTurnMessage;

        //Means its the players Turn 
        if(turnMessage.playerID == PlayerManager.Instance.CurrentPlayer.playerID)
            sideMenu.SlideMenu();
    }

    public void SwitchToGamePanel(MessageHeader packet)
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);
    }
}
