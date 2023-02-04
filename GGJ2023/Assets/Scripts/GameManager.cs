using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public StaticResourceManager resourceManager;
    public NetworkingManager networkingManager;

    // Per-map game controller instance
    public GameController gameController{get; set;}

    void Awake()
    {
        // Destroy extsting object
        if (FindObjectsOfType<GameManager>().Length >= 2) {
            Destroy(this.gameObject);
            return;
        }

        networkingManager.Initialize();

    }

    void Start()
    {
        // Do generate map in Start because need reference to controller
    }

    public TileManager Game_GetTilemapManager()
    {
        if (gameController == null)
        {
            return null;
        }

        return gameController.tileManager;
    }

    public MapGenerator Game_GetMapGenerator()
    {
        if (gameController == null)
        {
            return null;
        }

        return gameController.mapGenerator;
    }

}
