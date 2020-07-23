using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timers;
using UnityEngine.UI;

public class SideMenu : MonoBehaviour
{
    private ClientBehaviour clientBehaviour;
    private bool SlideOut = false;
    [SerializeField]
    private Animator menuAnimator;
    [SerializeField]
    private Animator[] doorsAnimators;
    private MoveRequest moveRequest;

    // Start is called before the first frame update
    private void Start()
    {
        clientBehaviour = FindObjectOfType<ClientBehaviour>();
    }

    public void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.M))
            SlideMenu();
#endif
    }

    public void SlideMenu()
    {
        SlideOut = !SlideOut;

        if (SlideOut)
            menuAnimator.SetTrigger("SlideMenu");
        else
            menuAnimator.SetTrigger("SlideMenuBack");

    }

    /// <summary>
    /// 0 North
    /// 1 East
    /// 2 South
    /// 3 West
    /// </summary>
    /// <param name="direction"></param>
    public void CreateMoveRequest(int direction)
    {
        if (SlideOut)
        {
            MoveRequest moveRequest = new MoveRequest()
            {
                direction = (Direction)direction
            };

            this.moveRequest = moveRequest;
            doorsAnimators[direction].SetTrigger("Open");
            TimerManager.Instance.AddTimer(SendMoveRequest, 1);
        }
    }

    public void SendMoveRequest()
    {
        clientBehaviour.SendRequest(moveRequest);
    }

}
