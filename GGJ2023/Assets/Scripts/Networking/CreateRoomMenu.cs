using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
//using Photon.Realtime;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private TMP_Text roomName;

    private RoomsUI cachedParentUI;

    [SerializeField]
    private RectTransform parentContainer;

    [SerializeField]
    private RoomListingsMenu listingsMenu;

    [SerializeField]
    private TMP_InputField nicknameText;

    void Start()
    {
        nicknameText.text = GetExampleUsername();
        OnUsernameTextChanged();
    }

    public void Initialize(RoomsUI parentUI)
    {
        cachedParentUI = parentUI;
    }

    private string GetExampleUsername()
    {
        return "Player_" + Random.Range(0, 1000);
    }

    public void OnClick_CreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }
        
        // Create Room
        // JoinOrCreateRoom
        Photon.Realtime.RoomOptions options = new Photon.Realtime.RoomOptions();
        PhotonNetwork.JoinOrCreateRoom(roomName.text, options, Photon.Realtime.TypedLobby.Default);

    }

    // https://doc-api.photonengine.com/en/pun/v2/interface_photon_1_1_realtime_1_1_i_matchmaking_callbacks.html
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();

        Debug.Log("Created room successfully!");
        cachedParentUI.SwitchToCurrentRoomMenu();

    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);

        Debug.Log("Room creation failed");

    }

    public void SetEnabled(bool enabled)
    {
        parentContainer.gameObject.SetActive(enabled);
        listingsMenu.SetEnabled(enabled);
    }

    public void OnUsernameTextChanged()
    {
        PhotonNetwork.NickName = nicknameText.text;
    }

}
