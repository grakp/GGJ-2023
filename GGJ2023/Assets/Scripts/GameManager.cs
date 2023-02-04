using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public TileManager tileManager;
    public MapGenerator mapGenerator;
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
        GenerateMap();
    }

    public void GenerateMap()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Photon is not connected. Map will not be replicated.");
            mapGenerator.GenerateMap(tileManager.GenerateMap);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                mapGenerator.GenerateMap(tileManager.GenerateMap);
                // TODO: replicate
            }
            else
            {
                // TODO: Wait for replication of world seed
            }
        }
    }
}
