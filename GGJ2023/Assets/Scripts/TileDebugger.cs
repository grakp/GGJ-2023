using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDebugger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TileInfo tile =GameManager.Instance.Game_GetTilemapManager().GetTileInWorld(transform.position);
        if (tile != null)
        {
            int corner = GameManager.Instance.Game_GetTilemapManager().GetTileIndexFromCorner(tile.corners);
            Debug.Log(tile.tileType + " " + corner);
        }

    }
}
