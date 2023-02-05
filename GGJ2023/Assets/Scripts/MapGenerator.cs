using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Procedural cave generation from tutorial: https://learn.unity.com/project/procedural-cave-generation-tutorial

public enum MapTileType {
    EMPTY = 0,
    WALL = 1,
    OTHER = 2,
    OUT_OF_BOUNDS = 3
};


[System.Serializable]
public class ObjectSpawnParams
{
    public int width = 1;
    public int height = 1;
};

public class MapGenerator : MonoBehaviour {

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;


    [Range(0,100)]
    public int randomFillPercent;

    public int emptyDistanceFromCenter = 5;

    MapTileType[,] map;


    [Header("Constants")]
    public int numSmoothIterations = 5;
    public int passagewaySize = 5;
    public int wallThresholdSize = 50;
    public int roomThresholdSize = 50;

    [Header("Objects")]
    public List<ObjectSpawnParams> spawnableObjects;

    public delegate void OnGenerateCallback(MapTileType[,] map);

    public Vector2 CoordToWorldPoint(Coord tile) {
        return new Vector2 (-width / 2 + .5f + tile.tileX, -height / 2 + .5f + tile.tileY);
    }

    public Coord NearestWorldPointToCoord(Vector3 loc) {
        Coord coord = new Coord(Mathf.FloorToInt(loc.x) + width/2, Mathf.FloorToInt(loc.z) + height/2 ) ;

        if (!IsInMapRange(coord.tileX, coord.tileY)) {
            coord.tileX = -1;
            coord.tileY = -1;
        }

        return coord;
    }

    public Vector3 NearestWorldPointToMapTile(Vector3 loc) {
        Coord nearestPoint = this.NearestWorldPointToCoord(loc);
        if (nearestPoint.tileX != -1) {
            Vector3 coordPosition = this.CoordToWorldPoint(nearestPoint);
            return coordPosition;
        } else {
            return Vector3.zero;
        }
    }

    public MapTileType[,] GetMap() {
        return this.map;
    }

    public MapTileType GetMapCoord(Coord coord) {
        if (!IsInMapRange(coord.tileX, coord.tileY)) {
            return MapTileType.OUT_OF_BOUNDS;
        }

        return this.map[coord.tileX, coord.tileY];
    }
    
    public void SetMapTile(Coord coord, MapTileType setType) {
        if (!IsInMapRange(coord.tileX, coord.tileY)) {
            return;
        }
        
        this.map[coord.tileX, coord.tileY] = setType;
    }


    public List<Vector3> GetRandomOpenLocations(int count, int radius = 1, bool ignoreCenter = false) {
        List<Vector3> locations = new List<Vector3>(); 
        List<Coord> openLocations =  this.GetRandomOpenCoords(count, radius, ignoreCenter);
        

        for (int i = 0; i < openLocations.Count; i++) {
            Vector3 theLocation = CoordToWorldPoint(openLocations[i]);
            locations.Add(theLocation);
        }

        return locations;
    }

    // Get random open location
    // Note that this isn't that efficient. Use sparingly.
    // Might be a good idea to cache this value later....
    public List<Coord> GetRandomOpenCoords(int count, int radius = 1, bool ignoreCenter = false) {
        List<Coord> openLocations = new List<Coord>();
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Coord curCoord = new Coord(x, y);
                if (IsEmptyLocation(curCoord, radius, ignoreCenter)) {
                    openLocations.Add(curCoord);
                }
            }
        }

        List<Coord> coords = new List<Coord>();
        if (count > openLocations.Count) {
            Debug.LogError("Not enough open locations: " + count + " " + openLocations.Count);
            return coords;
        }


        openLocations.Shuffle<Coord>(NetworkingManager.worldSeedRandom);

        for (int i = 0; i < count; i++) {
            coords.Add(openLocations[i]);
        }

        return coords;
    }



    public bool IsEmptyLocation(Coord loc, int radius, bool ignoreCenter = false) {
        if (!IsInMapRange(loc.tileX, loc.tileY) || map[loc.tileX, loc.tileY] == MapTileType.WALL) return false;
        
        int centerX = width/2;
        int centerY = height/2;

        if (ignoreCenter && IntersectsSquares(loc.tileX - radius, loc.tileX + radius, loc.tileY - radius, loc.tileY + radius,
                                                centerX - this.emptyDistanceFromCenter, centerX + this.emptyDistanceFromCenter, centerY - this.emptyDistanceFromCenter, centerY + this.emptyDistanceFromCenter)) {
                                                    return false;
                                                }

        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                int drawX = loc.tileX + x;
                int drawY = loc.tileY + y;

                if (!IsInMapRange(drawX, drawY)) {
                    return false;
                }

                if (map[drawX, drawY] != MapTileType.EMPTY) {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IntersectsSquares( int s1_x1, int s1_x2, int s1_y1, int s1_y2,
                                    int s2_x1, int s2_x2, int s2_y1, int s2_y2) {
                                        return s1_x2 >= s2_x1 && s1_x1 <= s2_x2 && s1_y2 >= s2_y1 && s1_y1 <= s2_y2;
                                    }

    public void GenerateMap(OnGenerateCallback Callback) {
        map = new MapTileType[width,height];
        
        // Fills map with random 1s and 0s
        RandomFillMap();

        // Make sure that regions with a lot of walls are walls themselves, and likewise for empty space
        for (int i = 0; i < this.numSmoothIterations; i ++) {
            SmoothMap();
        }

        // Draw circle in the center of the screen
        DrawCircle(new Coord(width/2, height/2), this.emptyDistanceFromCenter);
        // Create rooms and regions
        ProcessMap ();

        int borderSize = 1;
        MapTileType[,] borderedMap = new MapTileType[width + borderSize * 2,height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x ++) {
            for (int y = 0; y < borderedMap.GetLength(1); y ++) {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
                    borderedMap[x,y] = map[x-borderSize,y-borderSize];
                }
                else {
                    borderedMap[x,y] = MapTileType.WALL;
                }
            }
        }

        if (Callback != null)
        {
            Callback(borderedMap);
        }
        
    }

    public bool isOpenEdgeTile(Coord coord) {
        if (coord.tileX <= 0 || coord.tileX >= width-1 || coord.tileY <= 0 || coord.tileY >= height-1) {
            return true;
        }

        if (this.map[coord.tileX, coord.tileY] == MapTileType.EMPTY) {
            return false;
        }

        if (this.IsInMapRange(coord.tileX - 1, coord.tileY) && this.map[coord.tileX - 1, coord.tileY] == MapTileType.EMPTY) {
            return true;
        }

        if (this.IsInMapRange(coord.tileX + 1, coord.tileY) && this.map[coord.tileX + 1, coord.tileY] == MapTileType.EMPTY) {
            return true;
        }

        if (this.IsInMapRange(coord.tileX , coord.tileY - 1) && this.map[coord.tileX, coord.tileY - 1]  == MapTileType.EMPTY) {
            return true;
        }

        if (this.IsInMapRange(coord.tileX, coord.tileY + 1) && this.map[coord.tileX, coord.tileY + 1] == MapTileType.EMPTY) {
            return true;
        }

        return false;
    }


    void ProcessMap() {
       
        List<List<Coord>> wallRegions = GetRegions (MapTileType.WALL);
        
      // No tiny wall regions allowed
        foreach (List<Coord> wallRegion in wallRegions) {
            if (wallRegion.Count < wallThresholdSize) {
                foreach (Coord tile in wallRegion) {
                    map[tile.tileX,tile.tileY] = MapTileType.EMPTY;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions (MapTileType.EMPTY);
        
        List<Room> survivingRooms = new List<Room> ();
        
        foreach (List<Coord> roomRegion in roomRegions) {
            // No tiny rooms allowed
            if (roomRegion.Count < roomThresholdSize) {
                foreach (Coord tile in roomRegion) {
                    map[tile.tileX,tile.tileY] = MapTileType.WALL;
                }
            }
            else {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        // sort rooms by size
        survivingRooms.Sort ();
        survivingRooms [0].isMainRoom = true;
        survivingRooms [0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms (survivingRooms);
    }

    // Connects the closest rooms
    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {

        List<Room> roomListA = new List<Room> ();
        List<Room> roomListB = new List<Room> ();


        // Get lists of possible rooms that can connect
        if (forceAccessibilityFromMainRoom) {
            foreach (Room room in allRooms) {
                if (room.isAccessibleFromMainRoom) {
                    roomListB.Add (room);
                } else {
                    roomListA.Add (room);
                }
            }
        } else {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord ();
        Coord bestTileB = new Coord ();
        Room bestRoomA = new Room ();
        Room bestRoomB = new Room ();
        bool possibleConnectionFound = false;

        // Test each room from roomListA to roomListB
        // Find the two non-connected rooms that are the closest together
        foreach (Room roomA in roomListA) {
            if (!forceAccessibilityFromMainRoom) {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0) {
                    continue;
                }
            }

            foreach (Room roomB in roomListB) {
                if (roomA == roomB || roomA.IsConnected(roomB)) {
                    continue;
                }

                // Find the edge tile of roomA that is closest to the edge tile to roomB
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA ++) {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB ++) {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow (tileA.tileX-tileB.tileX,2) + Mathf.Pow (tileA.tileY-tileB.tileY,2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
                // Create a passage from roomA to roomB
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }


        // Edge cases
        if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom) {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
        Room.ConnectRooms (roomA, roomB);
        Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);


        // Get a line from room A to room B from the tiles, and then draw a circle for each point
        List<Coord> line = GetLine (tileA, tileB);
        foreach (Coord c in line) {
            DrawCircle(c,this.passagewaySize);
        }
    }

    // Draws a circle originating from c
    void DrawCircle(Coord c, int r) {
        for (int x = -r; x <= r; x++) {
            for (int y = -r; y <= r; y++) {
                if (x*x + y*y <= r*r) {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY)) {
                        map[drawX,drawY] = 0;
                    }
                }
            }
        }
    }

    // Get a list of coordinates representing a ling
    List<Coord> GetLine(Coord from, Coord to) {
        List<Coord> line = new List<Coord> ();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign (dx);
        int gradientStep = Math.Sign (dy);

        int longest = Mathf.Abs (dx);
        int shortest = Mathf.Abs (dy);

        if (longest < shortest) {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign (dy);
            gradientStep = Math.Sign (dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i =0; i < longest; i ++) {
            line.Add(new Coord(x,y));

            if (inverted) {
                y += step;
            }
            else {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest) {
                if (inverted) {
                    x += gradientStep;
                }
                else {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }



     // Gets all regions (i.e. all islands) in the map
    List<List<Coord>> GetRegions(MapTileType tileType) {
        List<List<Coord>> regions = new List<List<Coord>> ();
        int[,] mapFlags = new int[width,height];

        for (int x = 0; x < width; x ++) {
            for (int y = 0; y < height; y ++) {
                if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
                    List<Coord> newRegion = GetRegionTiles(x,y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion) {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    // Gets the tiles for a single "region" (i.e. an island)
    List<Coord> GetRegionTiles(int startX, int startY) {
        List<Coord> tiles = new List<Coord> ();
        int[,] mapFlags = new int[width,height];
        MapTileType tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord> ();
        queue.Enqueue (new Coord (startX, startY));
        mapFlags [startX, startY] = 1;

        while (queue.Count > 0) {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
                    if (IsInMapRange(x,y) && (y == tile.tileY || x == tile.tileX)) {
                        if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
                            mapFlags[x,y] = 1;
                            queue.Enqueue(new Coord(x,y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }


    // Fills map with random 1s and 0s
    void RandomFillMap() {
        if (useRandomSeed) {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        if (Photon.Pun.PhotonNetwork.IsConnected)
        {
           pseudoRandom = NetworkingManager.worldSeedRandom;
        }

        if (pseudoRandom == null)
        {
           Debug.LogError("Random seed not initialized properly: Psuedorandom null Error!");
        }

        for (int x = 0; x < width; x ++) {
            for (int y = 0; y < height; y ++) {

                if (x == 0 || x == width-1 || y == 0 || y == height -1) {
                    map[x,y] = MapTileType.WALL;
                }
                else {
                    map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent)? MapTileType.WALL: MapTileType.EMPTY;
                }
            }
        }
    }

    // Make sure that regions with a lot of walls are walls themselves, and likewise for empty space
    void SmoothMap() {
        for (int x = 0; x < width; x ++) {
            for (int y = 0; y < height; y ++) {
                int neighbourWallTiles = GetSurroundingWallCount(x,y);

                if (neighbourWallTiles > 4)
                    map[x,y] = MapTileType.WALL;
                else if (neighbourWallTiles < 4)
                    map[x,y] = MapTileType.EMPTY;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY) {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
                if (IsInMapRange(neighbourX,neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        if (map[neighbourX,neighbourY] == MapTileType.WALL) {
                            wallCount++;
                        }
                    }
                }
                else {
                    wallCount ++;
                }
            }
        }

        return wallCount;
    }

    public struct Coord {
        public int tileX;
        public int tileY;

        public Coord(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }


    public class Room : IComparable<Room> {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room() {
        }

        public Room(List<Coord> roomTiles, MapTileType[,] map) {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles) {
                for (int x = tile.tileX-1; x <= tile.tileX+1; x++) {
                    for (int y = tile.tileY-1; y <= tile.tileY+1; y++) {
                        if (x == tile.tileX || y == tile.tileY) {
                            if (map[x,y] == MapTileType.WALL) {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom() {
            if (!isAccessibleFromMainRoom) {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms) {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB) {
            if (roomA.isAccessibleFromMainRoom) {
                roomB.SetAccessibleFromMainRoom ();
            } else if (roomB.isAccessibleFromMainRoom) {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add (roomB);
            roomB.connectedRooms.Add (roomA);
        }

        public bool IsConnected(Room otherRoom) {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom) {
            return otherRoom.roomSize.CompareTo (roomSize);
        }
    }

}