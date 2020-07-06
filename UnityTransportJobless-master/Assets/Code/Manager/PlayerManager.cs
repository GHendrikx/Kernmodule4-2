
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : Singleton<PlayerManager>
{
    //Server
    public List<Players> players = new List<Players>();
    //Client
    public Players CurrentPlayer;
}