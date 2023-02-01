using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType
{
    Ground,
    Grass,
    Prop,
    Other
};

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
    public Vector2Int positionInArray;
    public Vector2Int cellPosition; // cellPosition != tileInfo position because of centering offset
    public TileDirectionType directionType;
    public int corners = 0x0000; // top, right, bottom, left

    public TiledGameObject tiledGameObject = null;
};

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private List<TileBase> floorTiles;

    [SerializeField]
    private TileBase grassTile;

    [SerializeField]
    private List<TileBase> alternativeGrassTiles;

    [SerializeField]
    private float alternativeGrassTileChance = 0.25f;

    private TileInfo[,] tileInfos;

    private int width;
    private int height;

    private HashSet<TileInfo> cachedEmptyTiles = new HashSet<TileInfo>();

    [SerializeField]
    private TiledGameObject TestPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameManager.Instance.GenerateMap();
        }
    }

    public void GenerateMap(MapTileType[,] map)
    {
        width = map.GetLength(0);
        height = map.GetLength(1);

        tileInfos = new TileInfo[width, height];

        int halfWidth = width / 2;
        int halfHeight = height / 2;
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tileInfos[i, j] =  new TileInfo();
                TileInfo info = tileInfos[i,j];
                info.positionInArray = new Vector2Int(i, j);
                info.cellPosition = ArrayToCellPosition(info.positionInArray);;
                info.tileType = map[i, j] == MapTileType.EMPTY ? TileType.Ground : TileType.Grass;

                //if (IsCollidingTile(info.tileType))
                //{
                //    info.shouldHaveCollision = true;
                //}
                // Assume all initial tiles don't have collision
                info.tiledGameObject = null;
                cachedEmptyTiles.Add(info);
            }
        }

        // Figure out the corners for the tile sprite
        HashSet<Vector2Int> floor = GetCellPositionsOfType(TileType.Ground);
        foreach (Vector2Int groundLocation in floor)
        {
            Vector2Int arrayPosition = CellToArrayPosition(groundLocation);
            TileInfo tile = tileInfos[arrayPosition.x, arrayPosition.y];
            
            Vector2Int upPosition = groundLocation + Vector2Int.up;
            if (!floor.Contains(upPosition))
            {
                tile.corners |= 0x1000;
            }

            Vector2Int rightPosition = groundLocation + Vector2Int.right;
            if (!floor.Contains(rightPosition))
            {
                tile.corners |= 0x0100;
            }

            Vector2Int downPosition = groundLocation + Vector2Int.down;
            if (!floor.Contains(downPosition))
            {
                tile.corners |= 0x0010;
            }

            Vector2Int leftPosition = groundLocation + Vector2Int.left;
            if (!floor.Contains(leftPosition))
            {
                tile.corners |= 0x0001;
            }
        }


        // Paint tiles
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TileInfo info = tileInfos[i, j];
                TileBase tile = null;
                if (info.tileType == TileType.Grass)
                {
                    float chance = Random.Range(0.0f, 1.0f);
                    if (chance < alternativeGrassTileChance)
                    {
                        int index = Random.Range(0, alternativeGrassTiles.Count);
                        tile = alternativeGrassTiles[index];
                    }
                    else
                    {
                        tile = grassTile;
                    }
                }
                else if (info.tileType == TileType.Ground)
                {
                    int tileCorner = GetTileIndexFromCorner(info.corners);
                    tile = floorTiles[tileCorner];
                }
                PaintSingleTile(tilemap, tile, info.cellPosition);
            }
        }

        Vector3 instantiatePosition = GetWorldPositionOfTileInArray(new Vector2Int(width/2, height/2));
        TiledGameObject newObject = Instantiate(TestPrefab, instantiatePosition, Quaternion.identity);
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }


/*
    // Note: Does not recalculate direction. Unsupported for now due to complexity
    void SetTile(int positionInArrayX, int positionInArrayY, TileType tileType)
    {
        TileInfo tile = tileInfos[positionInArrayX, positionInArrayY];
        if (cachedEmptyTiles.Contains(tile))
        {
            cachedEmptyTiles.Remove(tile);
        }

        tile.tileType = tileType;

        if (IsGroundTile(tileType))
        {
            cachedEmptyTiles.Add(tile);
        }
    }
    */

    public TileInfo GetTileInWorld(Vector3 worldPosition)
    {
        Vector3Int positionCoords = tilemap.WorldToCell(worldPosition);
        Vector2Int arrayPosition = CellToArrayPosition(new Vector2Int(positionCoords.x, positionCoords.y));
        return tileInfos[arrayPosition.x, arrayPosition.y];
    }

    Vector2Int CellToArrayPosition(Vector2Int cellPosition)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        return new Vector2Int(cellPosition.x +  halfWidth, cellPosition.y + halfHeight);
    }

    Vector2Int ArrayToCellPosition(Vector2Int arrayPosition)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        return new Vector2Int(arrayPosition.x - halfWidth, arrayPosition.y - halfHeight);
    }

    public Vector3 GetWorldPositionOfTileInArray(Vector2Int arrayPosition)
    {
        Vector2Int cellPosition = ArrayToCellPosition(arrayPosition);
        return GetWorldPositionOfTileCell(cellPosition);
    }

    public Vector3 GetWorldPositionOfTileCell(Vector2Int cellPosition)
    {
        return tilemap.CellToWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));
    }

    private HashSet<Vector2Int> GetCellPositionsOfType(TileType tileType)
    {
        HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                TileInfo info = tileInfos[i,j];
                if (info.tileType == tileType)
                {
                    positions.Add(info.cellPosition);
                }
            }
        }

        return positions;
    }


    public int GetTileIndexFromCorner(int corner)
    {
        int total = 0;
        if ((corner & 0x1000) != 0)
        {
            total += 8;
        }

        if ((corner & 0x0100) != 0)
        {
            total += 4;
        }

        if ((corner & 0x0010) != 0)
        {
            total += 2;
        }

        if ((corner & 0x0001) != 0)
        {
            total += 1;
        }

        return total;
    }
}
