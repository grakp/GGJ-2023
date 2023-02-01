using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public TileManager tileManager;
    public MapGenerator mapGenerator;

    void Awake()
    {
        GenerateMap();
    }

    void Start()
    {

    }

    public void GenerateMap()
    {
        mapGenerator.GenerateMap(tileManager.GenerateMap);
    }
}
