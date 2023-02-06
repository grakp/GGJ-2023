using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Photon.Realtime;

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

    // Special tiles are tiles that should be left empty.
    public bool specialTile = false;

    public bool IsEmptyTile()
    {
        return tiledGameObject == null && !specialTile;
    }
};

[System.Serializable]
public class TileSpawnParams
{
    public TiledGameObject tiledObjectPrefab;
    public int numObjects = 10;
};



[System.Serializable]
public class EnemySpawnParams
{
    public AiController enemyPrefab;
    public int numObjects = 10;
    public Vector2Int size = Vector2Int.one;
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
    private List<TileSpawnParams> spawnParams;

    [SerializeField]
    private int emptyDistanceFromCenter = 5;

    [Header("Water")]

    [SerializeField]
    private TiledGameObject waterTilePrefab;

    [SerializeField]
    private int numRivers = 10;

    [SerializeField]
    private int waterWalkIterations = 2;

    [SerializeField]
    private int waterWalkLength = 4;

    [SerializeField]
    private bool waterWalkStartRandomlyEachIteration = true;

    private List<Vector3> playerStartLocations = new List<Vector3>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.Instance.gameController.GenerateMap();
        }
    }

    public void GenerateMap(MapTileType[,] map)
    {
        width = map.GetLength(0);
        height = map.GetLength(1);

        tileInfos = new TileInfo[width, height];

        int halfWidth = width / 2;
        int halfHeight = height / 2;

        Vector2Int middle = new Vector2Int(halfWidth, halfHeight);
        
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tileInfos[i, j] =  new TileInfo();
                TileInfo info = tileInfos[i,j];
                info.positionInArray = new Vector2Int(i, j);
                info.cellPosition = ArrayToCellPosition(info.positionInArray);;
                info.tileType = map[i, j] == MapTileType.EMPTY ? TileType.Ground : TileType.Grass;

                int distance = TileDistance(info.positionInArray, middle);
                if (distance < emptyDistanceFromCenter)
                {
                    info.specialTile = true;
                }


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
                    float chance = NetworkingManager.RandomRangeUsingWorldSeed(0.0f, 1.0f);
                    if (chance < alternativeGrassTileChance)
                    {
                        int index = NetworkingManager.RandomRangeUsingWorldSeed(0, alternativeGrassTiles.Count);
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

        // Vector3 instantiatePosition = GetWorldPositionOfTileInArray(new Vector2Int(width/2, height/2));
        // TiledGameObject newObject = Instantiate(TestPrefab, instantiatePosition, Quaternion.identity);
        // Water will be spawned like an object, but it has a special algorthm, so do it first
        {

            HashSet<Vector2Int> waterTileLocations = new HashSet<Vector2Int>();
            Dictionary<Vector2Int, TiledGameObject> waterTileObjects = new  Dictionary<Vector2Int, TiledGameObject>();

            for (int i = 0; i < numRivers; i++)
            {
                if (i >= cachedEmptyTiles.Count)
                {
                    break;
                }

                List<TileInfo> emptyTileList = cachedEmptyTiles.ToList();
                emptyTileList.Shuffle(NetworkingManager.worldSeedRandom);
                TileInfo tile = emptyTileList[i];

                HashSet<Vector2Int> riverTiles = RunRandomWalk(tile.positionInArray, waterWalkIterations, waterWalkLength, waterWalkStartRandomlyEachIteration);
                foreach (Vector2Int riverTile in riverTiles)
                {
                    Vector3 instantiatePosition = GetWorldPositionOfTileInArray(riverTile);
                    TiledGameObject newObject = Instantiate(waterTilePrefab, instantiatePosition, Quaternion.identity);
                    newObject.transform.SetParent(GameManager.Instance.gameController.spawnedObjectParent);
                    tile = tileInfos[riverTile.x, riverTile.y];
                    newObject.Initialize(tile);
                    SetTileInUse(newObject, tile);

                    waterTileLocations.Add(tile.positionInArray);
                    waterTileObjects.Add(tile.positionInArray, newObject);
                }
            }

            // Set water tile corners
            foreach (Vector2Int waterLocation in waterTileLocations)
            {
                Vector2Int arrayPosition = waterLocation;
                TileInfo tile = tileInfos[arrayPosition.x, arrayPosition.y];

                int corner = 0;

                Vector2Int upPosition = waterLocation + Vector2Int.up;
                if (!waterTileLocations.Contains(upPosition))
                {
                    corner |= 0x1000;
                }

                Vector2Int rightPosition = waterLocation + Vector2Int.right;
                if (!waterTileLocations.Contains(rightPosition))
                {
                    corner |= 0x0100;
                }

                Vector2Int downPosition = waterLocation + Vector2Int.down;
                if (!waterTileLocations.Contains(downPosition))
                {
                    corner |= 0x0010;
                }

                Vector2Int leftPosition = waterLocation + Vector2Int.left;
                if (!waterTileLocations.Contains(leftPosition))
                {
                    corner |= 0x0001;
                }

                TiledGameObject waterObject = waterTileObjects[waterLocation];
                int cornerIndex = GetTileIndexFromCorner(corner);
                waterObject.SetCornerTile(cornerIndex);
            }
        }
        
        foreach (TileSpawnParams param in spawnParams)
        {
            if (param.tiledObjectPrefab == null)
            {
                Debug.LogWarning("Prefab is null!");
            }

            for (int i = 0; i < param.numObjects; i++)
            {
                List<TileInfo> emptyTileList = cachedEmptyTiles.ToList();
                emptyTileList.Shuffle(NetworkingManager.worldSeedRandom);

                int foundTileIndex = -1;
                for (int j = emptyTileList.Count - 1; j >= 0; j--)
                {
                    TileInfo tile = emptyTileList[j];
                    if (CanPlaceObjectOnTile(param.tiledObjectPrefab, tile))
                    {
                        foundTileIndex = j;
                        break;
                    }
                    else
                    {
                        emptyTileList.RemoveAt(j);
                    }
                }

                if (foundTileIndex != -1)
                {
                    TileInfo tile = emptyTileList[foundTileIndex];
                    Vector3 instantiatePosition = GetWorldPositionOfTileInArray(tile.positionInArray);
                    TiledGameObject newObject = Instantiate(param.tiledObjectPrefab, instantiatePosition, Quaternion.identity);
                    newObject.transform.SetParent(GameManager.Instance.gameController.spawnedObjectParent);
                    newObject.Initialize(tile);
                    SetTileInUse(newObject, tile);
                }
                else
                {
                    Debug.LogError("Unable to place: " + param.tiledObjectPrefab.name);
                }
            }


        }




        CalculatePlayerStartLocations();
        //cachedEmptyTiles = new HashSet<TileInfo>(emptyTileList);
    }

    public void SetTileInUse(TiledGameObject obj, TileInfo tile)
    {
        Vector2Int tileBounds = GetTiledGameObjectBounds(obj);
        Vector2Int originInArray = tile.positionInArray;
        for (int i = originInArray.x; i < originInArray.x + tileBounds.x; i++)
        {
            for (int j = originInArray.y; j < originInArray.y + tileBounds.y; j++)
            {
                TileInfo tileInfo = GetTileInfoInArraySafe(i, j);
                if (tileInfo == null)
                {
                    Debug.LogError("Should not be the case!");
                    return;
                }
                if (!tileInfo.IsEmptyTile())
                {
                    Debug.LogError("Should not be the case!");
                }
                else
                {
                    tileInfo.tiledGameObject = obj;
                }

                cachedEmptyTiles.Remove(tileInfo);
            }
        }
    }

    public void RemoveTileInUse(TiledGameObject obj, TileInfo tile)
    {
        Vector2Int tileBounds = GetTiledGameObjectBounds(obj);
        Vector2Int originInArray = tile.positionInArray;
        for (int i = originInArray.x; i < originInArray.x + tileBounds.x; i++)
        {
            for (int j = originInArray.y; j < originInArray.y + tileBounds.y; j++)
            {
                TileInfo tileInfo = GetTileInfoInArraySafe(i, j);
                if (tileInfo == null)
                {
                    Debug.LogError("Should not be the case!");
                    return;
                }
                if (tileInfo.IsEmptyTile())
                {
                    Debug.LogError("Should not be the case!");
                }
                else
                {
                    tileInfo.tiledGameObject = null;
                    tileInfo.specialTile = false;
                }

                cachedEmptyTiles.Add(tile);
            }
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    private bool CanPlaceObjectOnTile(TiledGameObject obj, TileInfo tile)
    {
        Vector2Int tileBounds = GetTiledGameObjectBounds(obj);
        return CanPlaceObjectOnTile(tileBounds, tile.positionInArray);
    }

    private bool CanPlaceObjectOnTile(Vector2Int tileBounds, Vector2Int originInArray)
    {
        for (int i = originInArray.x; i < originInArray.x + tileBounds.x; i++)
        {
            for (int j = originInArray.y; j < originInArray.y + tileBounds.y; j++)
            {
                TileInfo tileInfo = GetTileInfoInArraySafe(i, j);
                if (tileInfo == null)
                {
                    return false;
                }

                if (!tileInfo.IsEmptyTile())
                {
                    return false;
                }

                // TODO: Add other restrictions here, such as not being able to be in certain places in case it blocks stuff?
            }
        }

        return true;
    }

    private Vector2Int GetTiledGameObjectBounds(TiledGameObject obj)
    {

        if (obj == null)
        {
            Debug.Log("Null");
        }
        BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            return Vector2Int.zero;
        }

        return new Vector2Int((int)boxCollider.size.x, (int)boxCollider.size.y);
    }

    public TileInfo GetTileInfoInArraySafe(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return null;
        }

        return tileInfos[x, y];
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

    private int TileDistance(Vector2Int Start, Vector2Int End)
    {
        int x = End.x - Start.x;
        int y = End.y - Start.y;
        return (int)Mathf.Sqrt( x*x + y*y);
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


    // Walk for water algorithm
    private HashSet<Vector2Int> RunRandomWalk(Vector2Int position, int iterations, int walkLength, bool startRandomlyEachIteration)
    {
        var currentPosition = position;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < iterations; i++)
        {
            HashSet<Vector2Int> path = SimpleRandomWalk(currentPosition, walkLength);
            if (path.Count == 0)
            {
                return floorPositions;
            }

            floorPositions.UnionWith(path);
            if (startRandomlyEachIteration)
                currentPosition = floorPositions.ElementAt(NetworkingManager.RandomRangeUsingWorldSeed(0, floorPositions.Count));
        }
        return floorPositions;
    }

    private HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        TileInfo tile = GetTileInfoInArraySafe(startPosition.x, startPosition.y);
        if (tile == null || !tile.IsEmptyTile())
        {
            return path;
        }

        path.Add(startPosition);
        Vector2Int previousPosition = startPosition;

        for (int i = 0; i < walkLength; i++)
        {
            List<Vector2Int> cardinalDirectionsList = Direction2D.cardinalDirectionsList;
            cardinalDirectionsList.Shuffle(NetworkingManager.worldSeedRandom);

            bool bFoundTile = false;
            Vector2Int newPosition = new Vector2Int();
            for (int j = 0; j < cardinalDirectionsList.Count; j++)
            {
                newPosition = previousPosition + cardinalDirectionsList[j];
                tile = GetTileInfoInArraySafe(newPosition.x, newPosition.y);
                if (tile == null || !tile.IsEmptyTile())
                {
                    continue;
                }
                else
                {
                    bFoundTile = true;
                    break;
                }
            }

            if (!bFoundTile)
            {
                return path;
            }
           
            
            path.Add(newPosition);
            previousPosition = newPosition;
        }
        return path;
    }

    public TileInfo GetRandomEmptyLocation()
    {
        List<TileInfo> emptyTileList = cachedEmptyTiles.ToList();

        int index = NetworkingManager.RandomRangeUsingWorldSeed(0, cachedEmptyTiles.Count);
        return emptyTileList[index];
    }


    public void CalculatePlayerStartLocations()
    {
        TileInfo randomSpot = GetRandomEmptyLocation();
        if (randomSpot == null)
        {
            return;
        }

        if (GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            playerStartLocations.Add(GetWorldPositionOfTileInArray(randomSpot.positionInArray));
            return;
        }

        Vector2Int origin = randomSpot.positionInArray;

        // Note: Actor Ids start at 1
        int numPlayers = Photon.Pun.PhotonNetwork.CurrentRoom.Players.Count;
        if (numPlayers == 0)
        {
            return;
        }

        List<Vector2Int> playerLocations = new List<Vector2Int>();
        for (int i = origin.x; i < width; i++)
        {
            for (int j = origin.y; j < width; j++)
            {
                TileInfo info = tileInfos[i, j];

                if (IsSpawnableLocation(new Vector2Int(i, j), new Vector2Int(1, 1)))
                {
                    playerLocations.Add(info.positionInArray);
                }

                if (playerLocations.Count >= numPlayers)
                {
                    break;
                }
            }

            if (playerLocations.Count >= numPlayers)
            {
                break;
            }
        }
        
        for (int i = 0; i < numPlayers; i++)
        {
            Vector2Int location = playerLocations[i];
            playerStartLocations.Add(GetWorldPositionOfTileInArray(location));
        }
    }

    public List<Vector3> GetPlayerStartLocations()
    {
        return playerStartLocations;
    }

    public TileInfo GetRandomSpawnableLocation(Vector2Int objectSize)
    {
        List<TileInfo> emptyTileList = cachedEmptyTiles.ToList();
        emptyTileList.Shuffle(NetworkingManager.worldSeedRandom);

        for (int i = 0; i < emptyTileList.Count; i++)
        {
            TileInfo tile = emptyTileList[i];
            if (IsSpawnableLocation(tile.positionInArray, objectSize))
            {
                return tile;
            }
        }

        return null;
    }

    public bool IsSpawnableLocation(Vector2Int positionInArray, Vector2Int objectSize)
    {
        if (positionInArray.x + objectSize.x >= width || positionInArray.y + objectSize.y >= height)
        {
            return false;
        }

        TileInfo info = GetTileInfoInArraySafe(positionInArray.x, positionInArray.y);
        for (int i = 0; i < objectSize.x; i++)
        {
            for (int j = 0; j < objectSize.y; j++)
            {
                TileInfo tile = tileInfos[positionInArray.x + i, positionInArray.y + j];
                if (!tile.IsEmptyTile())
                {
                    return false;
                }
            }
        }

        // Finally, do a collision check with already placed units on the map
        Vector2 worldPosition = GetWorldPositionOfTileInArray(positionInArray);
        Vector2 centeredPosition = worldPosition + new Vector2(objectSize.x / 2.0f, objectSize.y / 2.0f);
        Collider2D[] collisions = Physics2D.OverlapBoxAll(centeredPosition, objectSize, 0);
        if (collisions == null || collisions.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < collisions.Length; i++)
        {
            if (!collisions[i].isTrigger)
            {
                return false;
            }
        }

        return true;
    }
}
