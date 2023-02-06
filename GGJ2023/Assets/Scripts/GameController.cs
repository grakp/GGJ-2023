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

    public bool debug_SpawnInCenterOfMap = false;

    [Header("Enemies")]
    [SerializeField]
    private List<EnemySpawnParams> enemySpawnParams;

    private List<AiController> enemies = new List<AiController>();

    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance.gameController = this;

        // If we haven't connected by now, we started in the game scene for debugging
        if (!PhotonNetwork.IsConnected)
        {
            GameManager.Instance.networkingManager.SetDebuggingMode(true);
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            SetWorldSeedPacket packet = new SetWorldSeedPacket();
            packet.worldSeed = NetworkingManager.worldSeed;
            GameManager.Instance.networkingManager.SendPacket(packet, Photon.Realtime.ReceiverGroup.Others);
        }

        if (GameManager.Instance.networkingManager.IsDebuggingMode || PhotonNetwork.IsMasterClient || GameManager.Instance.networkingManager.HasSetWorldSeed())
        {
            Initialize();
        }
        else
        {
            StartCoroutine(WaitForServerResponseAndInitialize());
        }

    }

    private void Initialize()
    {
        GenerateMap();
        GeneratePlayer();
        GenerateEnemies();
    }

    private IEnumerator WaitForServerResponseAndInitialize()
    {

        while (!GameManager.Instance.networkingManager.HasSetWorldSeed())
        {
            yield return null;
        }

        Initialize();
    }

    public void GenerateMap()
    {
        mapGenerator.GenerateMap(tileManager.GenerateMap);
    }

    private void GeneratePlayer()
    {
        // Add the player info first because instnatiate callback is not null on callback
        PlayerController player = null;
        if (GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            GamePlayerInfo newInfo = new GamePlayerInfo();
            List<Vector3> spawnLocations = tileManager.GetPlayerStartLocations();

            Vector3 spawnLocation = spawnLocations[0];

            if (debug_SpawnInCenterOfMap)
            {
                spawnLocation = Vector3.zero;
            }

            GameObject playerObj = Instantiate(GameManager.Instance.resourceManager.playerPrefab.gameObject, spawnLocation, Quaternion.identity);
            player = playerObj.GetComponent<PlayerController>();

            newInfo.controller = player;
            players.Add(newInfo);

        }
        else
        {
            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            List<Vector3> spawnLocations = tileManager.GetPlayerStartLocations();
            Vector3 spawnLocation = spawnLocations[localActorNumber - 1];
            if (debug_SpawnInCenterOfMap)
            {
                spawnLocation = Vector3.zero + Vector3.right * (localActorNumber - 1);
            }

            GameObject prefab = GameManager.Instance.resourceManager.playerPrefab.gameObject;
            GameObject playerObj = NetworkingSingleton.NetworkInstantiate(prefab, spawnLocation, Quaternion.identity);
            // Rest will be done in PlayerController
        }
    }

    public PlayerController GetMyPlayer()
    {
        if (players.Count == 0)
        {
            return null;
        }

        return players[0].controller;
    }

    public List<GamePlayerInfo> GetAllPlayers()
    {
        List<GamePlayerInfo> controllers = new List<GamePlayerInfo>();
        for (int i = 0; i < players.Count; i++)
        {
            controllers.Add(players[i]);
        }

        return controllers;
    } 

    public void AddPlayer(PlayerController player, Photon.Realtime.Player networkInfo)
    {
        GamePlayerInfo newInfo = new GamePlayerInfo();
        newInfo.controller = player;
        newInfo.controller.actorNumber = networkInfo.ActorNumber;
        newInfo.playerNetworkInfo = networkInfo;

        newInfo.controller.SetPlayerName(networkInfo.NickName);
        newInfo.controller.gameObject.name = "Player_ " + networkInfo.NickName;

        if (player.photonView.IsMine)
        {
            // Always have the local player first
            players.Insert(0, newInfo);
        }
        else
        {
            players.Add(newInfo);
        }

        players.Add(newInfo);
    }

    public GamePlayerInfo GetPlayerFromActorNumber(int actorNumber)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerNetworkInfo != null && players[i].playerNetworkInfo.ActorNumber == actorNumber)
            {
                return players[i];
            }
        }

        return null;
    }

    public void SpawnEnemy(AiController enemyPrefab, Vector3 spawnLocation)
    {
        if (GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            AiController enemy = Instantiate<AiController>(enemyPrefab, spawnLocation, Quaternion.identity);
        }
        else if (PhotonNetwork.IsMasterClient)
        {
            // Only the server creates enemies
            NetworkingSingleton.NetworkInstantiate(enemyPrefab.gameObject, spawnLocation, Quaternion.identity);
        }
    }

    public void AddEnemy(AiController enemy)
    {
        enemies.Add(enemy);
    }

    public void ReleaseEnemy(AiController enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    private void GenerateEnemies()
    {
        if (!GameManager.Instance.networkingManager.IsDebuggingMode && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        for (int i = 0; i < enemySpawnParams.Count; i++)
        {
            EnemySpawnParams spawnParams = enemySpawnParams[i];
            for (int j = 0; j < spawnParams.numObjects; j++)
            {
                TileInfo randomLocation = tileManager.GetRandomSpawnableLocation(spawnParams.size);
                if (randomLocation == null)
                {
                    Debug.Log("Failed to spawn!");
                    continue;
                }

                Vector3 worldPosition = tileManager.GetWorldPositionOfTileInArray(randomLocation.positionInArray);
                SpawnEnemy(enemySpawnParams[i].enemyPrefab, worldPosition);
            }
        }
    }

}

