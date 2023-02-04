using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform spawnedObjectParent;

    public UIController uiController;
    public PlayerController player;

    public TileManager tileManager;

    public MapGenerator mapGenerator;

    
    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance.gameController = this;
    }

    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
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

