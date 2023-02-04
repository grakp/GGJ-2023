using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private RoomsUI parentUI;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private RoomListing roomListing;

    private List<RoomListing> roomListings = new List<RoomListing>();

    public bool isEnabled = true;

    public void SetEnabled(bool enabled)
    {
        if (isEnabled == enabled)
        {
            return;
        }

        isEnabled = enabled;

        if (enabled)
        {
        }
        else
        {
            roomListings.Clear();

            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                int index = roomListings.FindIndex(x => x.roomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(roomListings[index].gameObject);
                    roomListings.RemoveAt(index);
                }
            }
            else
            {
                RoomListing listing = Instantiate(roomListing, content);
                if (listing != null)
                {
                    listing.SetRoomInfo(info);
                    roomListings.Add(listing);
                }
            }
        }
    }


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        parentUI.SwitchToCurrentRoomMenu();
    }
}
