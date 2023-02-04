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
        createRoomMenu.SetEnabled(true);
        currentRoomMenu.SetEnabled(false);
    }

    public void SwitchToCurrentRoomMenu()
    {
        createRoomMenu.SetEnabled(false);
        currentRoomMenu.SetEnabled(true);
    }
}
