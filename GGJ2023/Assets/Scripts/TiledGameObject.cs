using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;

public class TiledGameObject : MonoBehaviour
{

    public TMP_Text debugCanvasText;

    protected TileInfo originTile = null;

    [Header("Corners")]

    [SerializeField]
    private List<TileBase> cornerSprites;

    [SerializeField]
    protected Tilemap tilemap;

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
        int test = 0x1110;
        int cornerTest = GetTileIndexFromCorner(test);
        Debug.Log("Corner test: " + cornerTest);
        originTile = tileInfo;
        if (debugCanvasText != null && debugCanvasText.gameObject.activeInHierarchy)
        {
            debugCanvasText.text = originTile.positionInArray.x + ", " + originTile.positionInArray.y;
        }

    }

    public void SetCornerTile(int cornerTileIndex)
    {
        TileBase tileSprite = cornerSprites[cornerTileIndex];
        PaintTile(tileSprite, originTile.cellPosition);
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

    protected void PaintTile(TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    protected int GetTileIndexFromCorner(int corner)
    {
        int total = 0;
        if ((corner & 0x1000) != 0)
        {
            total += 8;
        }

        if ((corner & 0x0100) != 0)
        {
            total += 4;
        }

        if ((corner & 0x0010) != 0)
        {
            total += 2;
        }

        if ((corner & 0x0001) != 0)
        {
            total += 1;
        }

        return total;
    }

}
