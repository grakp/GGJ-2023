using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiledGameObject : MonoBehaviour
{

    protected TileInfo originTile = null;

    void OnDestroy()
    {
        if (originTile != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.tileManager != null)
            {
                GameManager.Instance.tileManager.RemoveTileInUse(this, originTile);
            }
        }

        originTile = null;
    }

    public virtual void Initialize(TileInfo tileInfo)
    {
        originTile = tileInfo;
    }

    public virtual void Interact(PlayerController player)
    {
    }

    public virtual void UnInteract()
    {
    }
}
