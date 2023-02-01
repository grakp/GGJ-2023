using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Floor,
    Wall,
    Interactable,
    Other
}

public enum TileDirectionType
{
    Default,
    Left,
    Right,
    Up,
    Down,
    BottomLeft,
    BottomRight,
    TopLeft,
    TopRight
};

public class TileInfo
{
    public TileType tileType;
    public TileDirectionType directionType;
    public bool shouldHaveCollision = false;
};

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private TileBase floorTile;

    [SerializeField]
    private TileBase wallTile;



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameManager.Instance.GenerateMap();
        }
    }

    public void GenerateMap(MapTileType[,] map)
    {
        int n = map.GetLength(0);
        int m = map.GetLength(1);

        int halfWidth = n / 2;
        int halfHeight = m / 2;
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                int xPos = i - halfWidth;
                int yPos = j - halfHeight;
                TileBase tile = map[i, j] == MapTileType.EMPTY ? floorTile : wallTile;
                PaintSingleTile(tilemap, tile, new Vector2Int(xPos, yPos));
            }
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

}
