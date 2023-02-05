using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public StaticResourceManager resourceManager;
    public NetworkingManager networkingManager;

    public Test_Connect connection;

    // Per-map game controller instance
    public GameController gameController{get; set;}


    void Awake()
    {
        // Destroy extsting object
        if (FindObjectsOfType<GameManager>().Length >= 2) {
            Debug.Log("Destroy manager gameobject");
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        connection.Initialize();
    }

    void Start()
    {
        // Do generate map in Start because need reference to controller
    }

    public void OnConnectedToNetworkRoom()
    {
        networkingManager.Initialize();
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
