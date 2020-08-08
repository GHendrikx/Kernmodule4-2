using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectLobby : MonoBehaviour
{
    ClientBehaviour clientBehaviour
    {
        get
        {
            return FindObjectOfType<ClientBehaviour>(); ;
        }
        set
        {
            clientBehaviour = value;
        }
    }

    public void Disconnect()
    {
        clientBehaviour.DisconnectPlayer();
    }
}
