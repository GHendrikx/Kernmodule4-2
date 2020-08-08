using UnityEngine;

public class Grid : MonoBehaviour
{
    Vector2 gridSize = new Vector2(10, 10);
    public Tile[,] tilesArray;

    public void GenerateGrid()
    {
        tilesArray = new Tile[(int)gridSize.x, (int)gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
                tilesArray[x, y] = new Tile(x, y);

        tilesArray[0, 0].Content = TileContent.None;
        tilesArray[0, 0].SetBeginOrExitTile(TileContent.Begin);

        tilesArray[(int)gridSize.x - 1, (int)gridSize.y - 1].Content = TileContent.None;
        tilesArray[(int)gridSize.x - 1, (int)gridSize.y - 1].SetBeginOrExitTile(TileContent.Exit);
    }

    public byte CheckNeighbors(int i)
    {
        Players currentPlayer = PlayerManager.Instance.Players[i];

        byte north = 0;
        byte east = 0;
        byte south = 0;
        byte west = 0;

        Vector2 currentPosition = currentPlayer.TilePosition;
        int x = (int)currentPosition.x;
        int y = (int)currentPosition.y;

        Tile southTile = null;
        Tile northTile = null;
        Tile eastTile = null;
        Tile westTile = null;

        //Y
        if(currentPosition.y + 1 < 10)
        northTile = tilesArray[(int)currentPosition.x, (int)currentPosition.y + 1];
        if((int)currentPosition.y - 1 > -1)
        southTile = tilesArray[(int)currentPosition.x, (int)currentPosition.y - 1];

        //X
        if ((int)currentPosition.x + 1 < 10)
        eastTile = tilesArray[(int)currentPosition.x + 1, (int)currentPosition.y];
        if(currentPosition.x - 1 > -1)
        westTile = tilesArray[(int)currentPosition.x - 1, (int)currentPosition.y];
        
        if (northTile != null)
            north = 0b00000001;
        if (southTile != null)
            south = 0b00000100;
        if (westTile != null)
            west = 0b00001000;
        if (eastTile != null)
            east = 0b00000010;

        byte answer = (byte)(north | east | south | west);

        return answer;
    }

    public TileContent TileContain(Vector2 position)
    {
        return tilesArray[(int)position.x, (int)position.y].Content;
    }
}

public class Tile
{
    public int X;
    public int Y;
    public TileContent Content;
    public int TreasureAmount;
    public int MonsterHealth;
    public bool ExitTile;
    public bool BeginTile;

    public Tile(int x, int y)
    {
        this.X = x;
        this.Y = y;
        RandomTileContent();
    }

    public void SetBeginOrExitTile(TileContent content)
    {
        if (content == TileContent.Exit)
        {
            ExitTile = true;
            Content = TileContent.Exit;
        }

        else if (content == TileContent.Begin)
        {
            BeginTile = true;
            content = TileContent.Begin;
        }
    }

    public void RandomTileContent()
    {
        int i = Random.Range(2, 5 + 1);
        Content = (TileContent)i;

        if (Content == TileContent.Treasure || Content == TileContent.Both)
            TreasureAmount = (int)Random.Range(10, 101);
        if (Content == TileContent.Monster || Content == TileContent.Both)
            MonsterHealth = 1;

    }
}

public enum TileContent
{
    Begin,
    Exit,
    Monster,
    Treasure,
    Both,
    None
}

