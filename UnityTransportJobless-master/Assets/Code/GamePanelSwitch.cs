using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePanelSwitch : MonoBehaviour
{
    [SerializeField]
    private GameObject[] panels;
    private GamePanel beginPanel = GamePanel.Connect;
    private void DeactivatePanels()
    {
        for (int i = 0; i < panels.Length; i++)
            panels[i].SetActive(false);
    }

    private void Start()
    {
        DeactivatePanels();
        panels[(int)beginPanel].SetActive(true);
    }

    public void SwitchToGamePanel(GamePanel panel)
    {
        DeactivatePanels();
        panels[(int)panel].SetActive(true);
    }


}

public enum GamePanel
{
    Connect,
    Lobby,
    Game
}
