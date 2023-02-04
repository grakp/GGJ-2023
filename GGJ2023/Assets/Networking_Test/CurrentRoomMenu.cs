using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CurrentRoomMenu : MonoBehaviour
{
    private RoomsUI cachedParentUI;

    [SerializeField]
    private RectTransform parentContainer;

    [SerializeField]
    private PlayerListingsMenu listingsMenu;

    public void Initialize(RoomsUI parentUI)
    {
        cachedParentUI = parentUI;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEnabled(bool enabled)
    {
        parentContainer.gameObject.SetActive(enabled);
        listingsMenu.SetEnabled(enabled);
    }


    public void OnButtonClicked_LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(true);

        cachedParentUI.SwitchToCreateRoomMenu();
    }
    
}
