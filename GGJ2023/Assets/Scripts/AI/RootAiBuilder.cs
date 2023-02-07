using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;

public class RootAiBuilder : MonoBehaviour // MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private TileManager _tileManager;

    public void Initialize(Vector2 dirToMove, float rotationWidth, TileInfo tile, bool isMaster) {
        _tileManager = GameManager.Instance.gameController.tileManager;

        TiledGameObject prefab = null;
        if (isMaster)
        {
            prefab = GameManager.Instance.resourceManager.rootMasterPrefab;
        }
        else
        {
            prefab = GameManager.Instance.resourceManager.rootNormalPrefab;
        }
        
        TiledGameObject obj = _tileManager.PlaceTile(prefab, tile);
    }
}
