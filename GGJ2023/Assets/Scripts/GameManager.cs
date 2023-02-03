using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public TileManager tileManager;
    public MapGenerator mapGenerator;
    public StaticResourceManager resourceManager;

    // Per-map game controller instance
    public GameController gameController{get; set;}

    void Awake()
    {
    }

    void Start()
    {
        // Do generate map in Start because need reference to controller
        GenerateMap();
    }

    public void GenerateMap()
    {
        mapGenerator.GenerateMap(tileManager.GenerateMap);
    }
}
