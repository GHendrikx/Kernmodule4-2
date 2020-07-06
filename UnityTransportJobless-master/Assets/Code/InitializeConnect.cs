using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitializeConnect : MonoBehaviour
{
    //PlayerName
    public InputField playerName;

    /// <summary>
    /// Spawning the Clientbehaviour for
    /// </summary>
    public void SpawnConnect()
    {
        string name = playerName.text;

        if(name == string.Empty)
            name = "Unknown #" + UnityEngine.Random.Range(0,float.MaxValue);
        GameObject go = new GameObject();
        ClientBehaviour c = go.AddComponent<ClientBehaviour>();
        c.playerName = name;
        GameManager.Instance.panelSwitch.SwitchToGamePanel(GamePanel.Lobby);
    }

    /// <summary>
    /// spawning the Hostbehaviour
    /// </summary>
    public void SpawnHost()
    {
        string name = playerName.text;
        if(name == string.Empty)
            name = "Unknown #" + UnityEngine.Random.Range(0, float.MaxValue);
        
        GameObject go = new GameObject();
        go.AddComponent<ServerBehaviour>();
        ClientBehaviour c = go.AddComponent<ClientBehaviour>();
        c.playerName = name;
        GameManager.Instance.panelSwitch.SwitchToGamePanel(GamePanel.Lobby);
    }
}
