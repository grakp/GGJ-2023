using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
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

    public void SetEnabled(bool enabled)
    {
        if (enabled && !firstTimeEnabled)
        {
           GetCurrentRoomPlayers();
           firstTimeEnabled = true;
        }
    }

    private void GetCurrentRoomPlayers()
    {
        Debug.Log("Num players in lobby: " + PhotonNetwork.CurrentRoom.Players.Count);
        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value);
        }
    }

    private void AddPlayerListing(Player player)
    {
        PlayerListing listing = Instantiate(playerListing, content);
        if (listing != null)
        {
            listing.SetPlayerInfo(player);
            playerListings.Add(listing);
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

}
