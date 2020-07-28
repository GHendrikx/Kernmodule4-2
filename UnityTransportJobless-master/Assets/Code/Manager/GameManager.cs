using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Grid currentGrid;
    
    // Update is called once per frame
    void Update()
    {
        // TODO: Remove this Reset HACK 
        if(Input.GetKeyUp(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    /// <summary>
    /// Packing info message for the player in this room.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public RoomInfoMessage MakeRoomInfoMessage(int i)
    {
        byte neighbors = currentGrid.CheckNeighbors(i);

        TileContent tileContent = currentGrid.TileContain(PlayerManager.Instance.Players[i].TilePosition);

        ushort treasure = 0;
        byte monster = 0;
        byte exit = 0;

        if (tileContent == TileContent.Treasure || tileContent == TileContent.Both)
            treasure = 1;
        if (tileContent == TileContent.Monster || tileContent == TileContent.Both)
            monster = 1;
        if (tileContent == TileContent.Exit)
            exit = 1;

        List<int> playersID = new List<int>();

        for (int j = 0; j < PlayerManager.Instance.Players.Count; j++)
            if (PlayerManager.Instance.Players[j].TilePosition == PlayerManager.Instance.Players[i].TilePosition)
            {
               playersID.Add(PlayerManager.Instance.Players[j].playerID);
            }

        var roomInfo = new RoomInfoMessage()
        {
            directions = neighbors,
            TreasureInRoom = treasure,
            ContainsMonster = monster,
            ContainsExit = exit,
            NumberOfOtherPlayers = (byte)playersID.Count,
            OtherPlayerIDs = playersID
        };

        return roomInfo;
    }
}
