using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PanelActivators : MonoBehaviour
{
    [SerializeField]
    private GameObject[] panels;
    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
            ResetPanels();
    }

    void ResetPanels()
    {
        panels[(int)Panels.PANEL_Connection].SetActive(true);
        panels[(int)Panels.PANEL_Lobby].SetActive(false);
        panels[(int)Panels.PANEL_Game].SetActive(false);
    }
}
public enum Panels
{
    PANEL_Connection,
    PANEL_Lobby,
    PANEL_Game
}