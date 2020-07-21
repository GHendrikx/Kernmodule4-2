using Assets.Code;
using System.Collections;
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
    
    public void SwitchToGamePanel(MessageHeader packet)
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);
    }
}
