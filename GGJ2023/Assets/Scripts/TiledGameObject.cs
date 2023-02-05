using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;

public class TiledGameObject : MonoBehaviour
{

    public TMP_Text debugCanvasText;

    protected TileInfo originTile = null;

    void OnDestroy()
    {
        if (originTile != null)
        {
            if (GameManager.Instance != null &&  GameManager.Instance.Game_GetTilemapManager() != null)
            {
                GameManager.Instance.Game_GetTilemapManager().RemoveTileInUse(this, originTile);
            }
        }

        originTile = null;
    }

    public virtual void Initialize(TileInfo tileInfo)
    {
        originTile = tileInfo;
        if (debugCanvasText != null && debugCanvasText.gameObject.activeInHierarchy)
        {
            debugCanvasText.text = originTile.positionInArray.x + ", " + originTile.positionInArray.y;
        }
    }

    public virtual void RequestInteract(PlayerController player)
    {
        if (GameManager.Instance.networkingManager.IsDebuggingMode || PhotonNetwork.IsMasterClient)
        {
            DoInteract(player);
        }

        InteractPacket interactPacket = new InteractPacket();
        interactPacket.actorNumber = player.actorNumber;
        interactPacket.locationX = originTile.positionInArray.x;
        interactPacket.locationY = originTile.positionInArray.y;
        GameManager.Instance.networkingManager.SendRequestPacket(interactPacket);
    }

    public virtual void DoInteract(PlayerController player)
    {
    }


    public virtual void UnInteract()
    {
    }
}
