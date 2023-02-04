using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private RoomsUI parentUI;


    [SerializeField]
    private Transform content;

    [SerializeField]
    private PlayerListing playerListing;

    private List<PlayerListing> playerListings = new List<PlayerListing>();

    bool firstTimeEnabled = false;

    public bool isEnabled = false;

    [SerializeField]
    private Button joinRoomButton;

    [SerializeField]
    private string sceneNameToMoveTo = "atu_NetworkingTestGameScene";

    public void SetEnabled(bool enabled)
    {
        if (isEnabled == enabled)
        {
            return;
        }

        isEnabled = enabled;

        if (enabled)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                joinRoomButton.interactable = false;
            }

            GetCurrentRoomPlayers();
        }
        else
        {
            playerListings.Clear();

            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void GetCurrentRoomPlayers()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
        {
            return;
        }

        Debug.Log("Num players in lobby: " + PhotonNetwork.CurrentRoom.Players.Count);
        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value);
        }
    }

    private void AddPlayerListing(Player player)
    {
        int index = playerListings.FindIndex(x=> x.player == player);
        if (index != -1)
        {
            playerListings[index].SetPlayerInfo(player);
        }
        else
        {
            PlayerListing listing = Instantiate(playerListing, content);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                playerListings.Add(listing);
            }

            // TODO: Cache player in a game manager
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        AddPlayerListing(newPlayer);

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
         int index = playerListings.FindIndex(x => x.player == otherPlayer);
         if (index != -1)
         {
            Destroy(playerListings[index].gameObject);
            playerListings.RemoveAt(index);
         }

    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        playerListings.Clear();

        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnClick_StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        //Scene nextScene = SceneManager.GetSceneByName("Networking_Test/atu_NetworkingTestGameScene");
        //Scene nextScene = SceneManager.GetSceneByBuildIndex(1);
        //Debug.Log(nextScene.name);

        //int index = SceneUtility.GetBuildIndexByScenePath("Networking_Test/atu_NetworkingTestGameScene");
        //Debug.Log(index);
        PhotonNetwork.LoadLevel(sceneNameToMoveTo);
        //SceneManager.LoadScene("atu_NetworkingTestGameScene");

        
    }

}
