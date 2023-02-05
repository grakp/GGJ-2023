using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsUI : MonoBehaviour
{
    public CreateRoomMenu createRoomMenu;
    public CurrentRoomMenu currentRoomMenu;

    void Awake()
    {
        createRoomMenu.Initialize(this);
        currentRoomMenu.Initialize(this);
    }


    public void SwitchToCreateRoomMenu()
    {
        createRoomMenu.OnUsernameTextChanged();
        createRoomMenu.SetEnabled(true);
        currentRoomMenu.SetEnabled(false);
    }

    public void SwitchToCurrentRoomMenu()
    {
        createRoomMenu.OnUsernameTextChanged();
        createRoomMenu.SetEnabled(false);
        currentRoomMenu.SetEnabled(true);
    }
}
