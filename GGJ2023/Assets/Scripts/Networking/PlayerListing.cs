using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class PlayerListing : MonoBehaviour
{

    [SerializeField]
    private TMP_Text text;
    public Player player{get; set;}
    
    public void SetPlayerInfo(Player player)
    {
        this.player = player;
        text.text = player.ActorNumber + ": " + player.NickName;
    }
}
