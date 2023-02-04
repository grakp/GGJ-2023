using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GameController : MonoBehaviour, IPunInstantiateMagicCallback
{
    public Transform spawnedObjectParent;

    public UIController uiController;

    public TileManager tileManager;

    public MapGenerator mapGenerator;

    private List<PlayerController> players = new List<PlayerController>();

    
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
            }
        }
    }

    private void GeneratePlayer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PlayerController player = (PlayerController)Instantiate(GameManager.Instance.resourceManager.playerPrefab, Vector3.zero, Quaternion.identity);
            players.Add(player);
            Debug.Log("Added player");
        }
        else
        {
            // TODO:
            //if (PhotonNetwork.IsMasterClient)
            //{
            //    mapGenerator.GenerateMap(tileManager.GenerateMap);
            //    // TODO: replicate
            //}
            //else
            //{
            //    // TODO: Wait for replication of world seed
            //}
        }
    }

    public PlayerController GetMyPlayer()
    {
        return players[0];
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // TODO make sure that self is/not included in list
        if (info.photonView.gameObject == null)
        {
            return;
        }

        PlayerController controller = info.photonView.gameObject.GetComponent<PlayerController>();
        if (controller != null)
        {
            players.Add(controller);
        }
    }
}

