using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public bool LOCAL;

    public Grid CurrentGrid;
    [SerializeField]
    private GameObject titleScreen;

    public Text Username;
    public InputField Password;

    public UserData myData;

    private IEnumerator Start()
    {
        yield return StartCoroutine(DatabaseManager.GetHttp("ServerLogin.php?server_id=" + DatabaseManager.ServerID + "&password=" + DatabaseManager.ServerPassword));
        DatabaseManager.sessionID = DatabaseManager.response;
    }


    // Update is called once per frame
    void Update()
    {
        // TODO: Remove this Reset HACK before Vincent sees it.
        if (Input.GetKeyUp(KeyCode.Escape))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // oops he already saw it.
    }

    public void Login()
    {
        StartCoroutine(CheckData());
    }

    public IEnumerator CheckData()
    {
        yield return StartCoroutine(DatabaseManager.GetHttp($"UserLogin.php?userid={Username.text}&password={Password.text}&session_id={DatabaseManager.sessionID}"));

        if (DatabaseManager.response != 0 + "")
        {
            myData = JsonUtility.FromJson<UserData>(DatabaseManager.response);
            ToggleLoginScreen();
        }
    }

    public void InsertScore(MessageHeader message)
    {
        EndGameMessage endGame = message as EndGameMessage;
        Debug.Log("EndGame");
        for (int i = 0; i < endGame.numberOfScores; i++)
            StartCoroutine(DatabaseManager.GetHttp($"InsertScore.php?User_ID={ PlayerManager.Instance.PlayersWhoLeft[endGame.playerID[i]].playerID} &Score={ endGame.highScorePairs[i] }&session_id={ DatabaseManager.sessionID }"));
        Debug.Log("For end");
        StatisticManager.Instance.UpdateStatistics();
    }

    public void ToggleLoginScreen() =>
        titleScreen.SetActive(false);

    /// <summary>
    /// Packing info message for the player in this room.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public RoomInfoMessage MakeRoomInfoMessage(int i)
    {
        byte neighbors = CurrentGrid.CheckNeighbors(i);

        TileContent tileContent = CurrentGrid.TileContain(PlayerManager.Instance.Players[i].TilePosition);

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
                playersID.Add(PlayerManager.Instance.Players[j].playerID);

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

    public void ExitGame()
    {
        Application.Quit();
    }
}
