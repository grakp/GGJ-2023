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
            GenerateMap();
            GeneratePlayer();
        }
        else
        {
            StartCoroutine(WaitForServerResponseAndInitialize());
        }

    }

    private IEnumerator WaitForServerResponseAndInitialize()
    {

        while (!GameManager.Instance.networkingManager.HasSetWorldSeed())
        {
            yield return null;
        }

        GenerateMap();
        GeneratePlayer();
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

            GameObject playerObj = Instantiate(GameManager.Instance.resourceManager.playerPrefab.gameObject, Vector3.zero, Quaternion.identity);
            player = playerObj.GetComponent<PlayerController>();

            newInfo.controller = player;
            players.Add(newInfo);

        }
        else
        {
            int localActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 spawnLocation = Vector3.right * localActorNumber;
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

}

