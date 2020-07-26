using Assets.Code;
using UnityEngine;
using UnityEngine.Timers;

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
        if (PlayerManager.Instance.CurrentPlayer != PlayerManager.Instance.Players[PlayerManager.Instance.PlayerIDWithTurn])
            return;

        DefendRequestMessage defendRequest = new DefendRequestMessage();
        clientBehaviour.SendRequest(defendRequest);
        PlayerManager.Instance.CurrentPlayer.Shield.SetActive(true);
    }

    /// <summary>
    /// Attackig the player
    /// </summary>
    public void SendAttackRequest()
    {
        if (PlayerManager.Instance.CurrentPlayer != PlayerManager.Instance.Players[PlayerManager.Instance.PlayerIDWithTurn])
            return;

        AttackRequestMessage attackRequest = new AttackRequestMessage();
        clientBehaviour.SendRequest(attackRequest);
        Players currentPlayer = PlayerManager.Instance.CurrentPlayer;
        Tile currentTile = GameManager.Instance.currentGrid.tilesArray[(int)currentPlayer.TilePosition.x, (int)currentPlayer.TilePosition.y];
        currentTile.MonsterHealth = 0;

        if (currentTile.Content == TileContent.Both)
            currentTile.Content = TileContent.Treasure;
        else
            currentTile.Content = TileContent.None;

        UIManager.Instance.monsterSprite.SetActive(false);
    }

    public void SendClaimTreasureRequest()
    {
        if (PlayerManager.Instance.CurrentPlayer != PlayerManager.Instance.Players[PlayerManager.Instance.PlayerIDWithTurn])
            return;

        ObtainTreasureMessage obtainTreasureMessage = new ObtainTreasureMessage();
        clientBehaviour.SendRequest(obtainTreasureMessage);
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
        Direction dir = (Direction)direction;

        MoveRequest moveRequest = new MoveRequest()
        {
            direction = dir
        };

        this.moveRequest = moveRequest;

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
        TimerManager.Instance.AddTimer(SendMoveRequest, 1);
    }

    public void SendMoveRequest()
    {
        PlayerManager.Instance.MovePlayer(moveRequest, PlayerManager.Instance.CurrentPlayer.playerID);
        clientBehaviour.SendRequest(moveRequest);
    }

    public void DisconnectPlayer()
    {
        clientBehaviour.DisconnectPlayer();
    }

}
