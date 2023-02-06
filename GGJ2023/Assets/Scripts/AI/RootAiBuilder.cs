using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;

public class RootAiBuilder : MonoBehaviour // MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public TiledGameObject rootPrefab;
    private TileManager _tileManager;

    public void Initialize(Vector2 dirToMove, float rotationWidth, TileInfo tile) {
        _tileManager = GameManager.Instance.gameController.tileManager;
        TiledGameObject obj = _tileManager.PlaceTile(rootPrefab, tile);
    }
}
