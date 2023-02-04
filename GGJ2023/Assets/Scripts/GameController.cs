using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GamePlayerInfo
{
    public PlayerController controller = null;
    public Photon.Realtime.Player playerNetworkInfo = null;
};

public class GameController : MonoBehaviour
{
    public Transform spawnedObjectParent;

    public UIController uiController;

    public TileManager tileManager;

    public MapGenerator mapGenerator;

    private List<GamePlayerInfo> players = new List<GamePlayerInfo>();

    
    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance.gameController = this;

        GenerateMap();
        GeneratePlayer();
    }

    void Start()
    {

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
                mapGenerator.GenerateMap(tileManager.GenerateMap);
            }
        }
    }

    private void GeneratePlayer()
    {
        // Add the player info first because instnatiate callback is not null on callback
        GamePlayerInfo newInfo = new GamePlayerInfo();
        players.Add(newInfo);

        PlayerController player = null;
        if (!PhotonNetwork.IsConnected)
        {
            GameObject playerObj = Instantiate(GameManager.Instance.resourceManager.playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            player = playerObj.GetComponent<PlayerController>();
        }
        else
        {

            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 spawnLocation = Vector3.right * localActorNumber;
            GameObject prefab = GameManager.Instance.resourceManager.playerPrefab.gameObject;
            GameObject playerObj = NetworkingSingleton.NetworkInstantiate(prefab, spawnLocation, Quaternion.identity);
            player = playerObj.GetComponent<PlayerController>();
        }

        if (player != null)
        {
            newInfo.controller = player;
        }
        else
        {
            Debug.LogError("Failed to instantiate player");
        }
    }

    public PlayerController GetMyPlayer()
    {
        return players[0].controller;
    }

    public void SetNetworkPlayerInfo(int index, Photon.Realtime.Player networkInfo)
    {
        players[index].playerNetworkInfo = networkInfo;
    }

    public void AddRemotePlayer(PlayerController player, Photon.Realtime.Player networkInfo)
    {
        GamePlayerInfo newInfo = new GamePlayerInfo();
        newInfo.controller = player;
        newInfo.playerNetworkInfo = networkInfo;
        players.Add(newInfo);
    }


}

