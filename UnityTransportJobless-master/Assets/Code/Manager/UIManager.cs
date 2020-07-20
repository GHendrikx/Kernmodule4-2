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
    [SerializeField]
    private GameObject lobbyPanel;
    [SerializeField]
    private GameObject gamePanel;
    private ServerBehaviour serverBehaviour
    {
        get
        {
            if (serverBehaviour == null)
                FindObjectOfType<ServerBehaviour>();
            return serverBehaviour;
        }
        set
        {
            serverBehaviour = value;
        }
    }

    private void Start()
    {
        playButton.onClick.AddListener(serverBehaviour.StartGame);
    }

    private void Update()
    {
        if(Content.transform.childCount > 1)
            playButton.interactable = true;
        else
            playButton.interactable = false;


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
    
    public void SwitchToGamePanel()
    {
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);
    }
}
