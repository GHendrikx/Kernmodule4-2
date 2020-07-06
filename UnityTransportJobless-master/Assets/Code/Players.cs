using System;
using UnityEngine;

[Serializable]
public class Players
{
    public int playerID;
    public string clientName;
    public uint clientColor;
    public Vector2 TilePosition;
    public uint treasureAmount;
    public bool Turn;
    public bool DefendOneTurn;

    public Players(int playerID, string clientName, uint clientColor)
    {
        this.playerID = playerID;
        this.clientName = clientName;
        this.clientColor = clientColor;
        this.TilePosition = new Vector2(0.0f, 0.0f);
        treasureAmount = 0;
    }
}
