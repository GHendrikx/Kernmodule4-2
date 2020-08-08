using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticManager : Singleton<StatisticManager>
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private GameObject statsPanel;

    public void UpdateStatistics()
    {
        StartCoroutine(ShowStatistics());
    }
    public IEnumerator ShowStatistics()
    {
        Debug.Log("Show Statics");
        yield return StartCoroutine(DatabaseManager.GetHttp("Statistics.php"));
        statsPanel.SetActive(true);
        DataBaseStatistic highscores = new DataBaseStatistic();
        highscores = JsonUtility.FromJson<DataBaseStatistic>(DatabaseManager.response);
        nameText.text = "";
        scoreText.text = "";

        foreach (DatabaseHighScore scores in highscores.topofmonth)
        {
            nameText.text += scores.Username + "\n";
            scoreText.text += scores.gemiddelde + "\n";
        }

    }


}

[System.Serializable]
public struct DataBaseStatistic
{
    public DatabaseHighScore[] topofmonth;
}

[System.Serializable]
public struct DatabaseHighScore
{
    public int gemiddelde;
    public int amountofwins;
    public string Username;
}