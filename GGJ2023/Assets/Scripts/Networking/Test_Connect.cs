using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class Test_Connect : MonoBehaviourPunCallbacks
{

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        Debug.Log("Connecting to server");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.0.1";
        string nickname = "Test_Nickname";

    #if UNITY_EDITOR
        nickname += "_Client";
    #else
        nickname += "_Host";
    #endif

        PhotonNetwork.NickName = nickname;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to server: " + PhotonNetwork.LocalPlayer.NickName);


        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }


    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected: " + cause.ToString());
    }
}
