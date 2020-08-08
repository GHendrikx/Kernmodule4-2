using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitializeConnect : MonoBehaviour
{
    //PlayerName
    public InputField playerName;
    //Ip Adress you want to connect to
    public InputField ipAdress;

    /// <summary>
    /// Spawning the Clientbehaviour for
    /// </summary>
    public void SpawnConnect()
    {
        string name = playerName.text;

        if(name == string.Empty)
            name = "Vincent #" + Mathf.RoundToInt(UnityEngine.Random.Range(0,1000));
        GameObject go = new GameObject();
        ClientBehaviour connect = go.AddComponent<ClientBehaviour>();

        if (ipAdress.text != "")
            connect.IPAdress = ipAdress.text;

        go.name = "ClientBehaviour";
        connect.playerName = name;
    }

    /// <summary>
    /// spawning the Hostbehaviour
    /// </summary>
    public void SpawnHost()
    {
        string name = playerName.text;
        if(name == string.Empty)
            name = "Vincent #" + Mathf.RoundToInt(UnityEngine.Random.Range(0, 1000));

        GameObject go = new GameObject();
        //adding serverbehaviour and set the playbutton onclick
        ServerBehaviour serverBehaviour = go.AddComponent<ServerBehaviour>();
        UIManager.Instance.PlayButton.onClick.AddListener(serverBehaviour.StartGame);
        
        //clientbehaviour
        ClientBehaviour connect = go.AddComponent<ClientBehaviour>();
        if (ipAdress.text == "")
        {
            serverBehaviour.IPAdress = ipAdress.text;
            connect.IPAdress = ipAdress.text;
        }
        go.name = "Server + Client";
        connect.playerName = name;
    }
}
