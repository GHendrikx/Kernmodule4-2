using Assets.Code;
using System;
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
    [SerializeField]
    private Button leaveButton;
    // Start is called before the first frame update
    private void Start()
    {
        clientBehaviour = FindObjectOfType<ClientBehaviour>();
        leaveButton.onClick.AddListener(() => SendLeaveRequest());
        leaveButton.gameObject.SetActive(false);
    }

    public void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.L))
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
    /// Let the player defend in the game showing the shield.
    /// </summary>
    public void SendDefendRequest()
    {
        DefendRequestMessage defendRequest = new DefendRequestMessage();
        clientBehaviour.SendRequest(defendRequest);
        PlayerManager.Instance.CurrentPlayer.Shield.SetActive(true);
    }

    /// <summary>
    /// Attackig the player
    /// </summary>
    public void SendAttackRequest()
    {
        AttackRequestMessage attackRequest = new AttackRequestMessage();
        clientBehaviour.SendRequest(attackRequest);
    }

    public void SendClaimTreasureRequest()
    {
        ClaimTreasureRequestMessage claimTreasureRequest = new ClaimTreasureRequestMessage();
        clientBehaviour.SendRequest(claimTreasureRequest);
    }


    public void SendLeaveRequest()
    {
        LeaveDungeonRequest leaveDungeon = new LeaveDungeonRequest();
        clientBehaviour.SendRequest(leaveDungeon);
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
        if (!SlideOut)
        {
            return;
        }

        Vector2 currentPosition = PlayerManager.Instance.CurrentPlayer.TilePosition;
        Debug.Log(PlayerManager.Instance.CurrentPlayer);
        Debug.Log(currentPosition);
        Debug.Log(GameManager.Instance.CurrentGrid);


        Direction dir = (Direction)direction;

        MoveRequest moveRequest = new MoveRequest()
        {
            direction = dir
        };

        this.moveRequest = moveRequest;
        TimerManager.Instance.AddTimer(DeactivateSprite, 1);

        if (PlayerManager.Instance.CurrentPlayer.playerID == PlayerManager.Instance.PlayerIDWithTurn)
        {
            switch (dir)
            {
                case Direction.North:
                    doorsAnimators[0].SetTrigger("Open");
                    break;
                case Direction.East:
                    doorsAnimators[1].SetTrigger("Open");

                    break;
                case Direction.South:
                    doorsAnimators[2].SetTrigger("Open");
                    break;
                case Direction.West:
                    doorsAnimators[3].SetTrigger("Open");
                    break;

            }
        }
        TimerManager.Instance.AddTimer(SendMoveRequest, 1);
    }

    private void DeactivateSprite()
    {
        for (int i = 0; i < PlayerManager.Instance.Players.Count; i++)
        {
            if (PlayerManager.Instance.Players[i] == PlayerManager.Instance.CurrentPlayer)
                continue;
            else
                if (PlayerManager.Instance.Players[i].Sprite != null)
                PlayerManager.Instance.Players[i].Sprite.gameObject.SetActive(false);
        }
    }

    public void SendMoveRequest()
    {
        clientBehaviour.SendRequest(moveRequest);
    }

    public void DisconnectPlayer()
    {
        clientBehaviour.DisconnectPlayer();
    }

}