using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    Vector2 gridSize = new Vector2(10, 10);
    public List<Tile> tiles = new List<Tile>();

    public void Start()
    {
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
                tiles.Add(new Tile(x, y));

        tiles[0].SetBeginOrExitTile(TileContent.Begin);

        tiles[tiles.Count - 1].SetBeginOrExitTile(TileContent.Exit);

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].Content == TileContent.Treasure || tiles[i].Content == TileContent.Both)
                tiles[i].RandomTreasureAmount = Random.Range(10, 101);
            if (tiles[i].Content == TileContent.Monster || tiles[i].Content == TileContent.Both)
                tiles[i].MonsterHealth = Random.Range(1, 6);
        }
    }

}
public class Tile
{
    public int X;
    public int Y;
    public TileContent Content;
    public int RandomTreasureAmount;
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
        int i = Random.Range(2, 4 + 1);
        Content = (TileContent)i;
    }
}

public enum TileContent
{
    Begin,
    Exit,
    Monster,
    Treasure,
    Both
}

