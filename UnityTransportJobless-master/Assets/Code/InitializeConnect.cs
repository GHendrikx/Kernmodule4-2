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
            name = "Unknown #" + UnityEngine.Random.Range(0,1000);
        GameObject go = new GameObject();
        ClientBehaviour c = go.AddComponent<ClientBehaviour>();
        go.name = "ClientBehaviour";
        c.playerName = name;
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
        //adding serverbehaviour and set the playbutton onclick
        ServerBehaviour serverBehaviour = go.AddComponent<ServerBehaviour>();
        UIManager.Instance.PlayButton.onClick.AddListener(serverBehaviour.StartGame);

        //clientbehaviour
        ClientBehaviour connect = go.AddComponent<ClientBehaviour>();
        go.name = "Server + Client";
        connect.playerName = name;
    }
}
