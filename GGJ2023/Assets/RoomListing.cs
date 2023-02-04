using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomListing : MonoBehaviour
{

    [SerializeField]
    private TMP_Text text;
    
    public void SetRoomInfo(RoomInfo roomInfo)
    {
        text.text = roomInfo.MaxPlayers + ", " + roomInfo.Name;
    }
}
