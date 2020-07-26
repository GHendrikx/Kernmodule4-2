using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectLobby : MonoBehaviour
{
    ClientBehaviour clientBehaviour
    {
        get
        {
            if (clientBehaviour == null)
                clientBehaviour = FindObjectOfType<ClientBehaviour>();
            return clientBehaviour;
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
