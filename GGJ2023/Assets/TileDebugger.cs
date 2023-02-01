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
        TileInfo tile = GameManager.Instance.tileManager.GetTileInWorld(transform.position);
        int corner = GameManager.Instance.tileManager.GetTileIndexFromCorner(tile.corners);
        Debug.Log(tile.tileType + " " + corner);
    }
}
