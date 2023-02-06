using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;

public class RootAi : MonoBehaviour // MonoBehaviourPun, IPunInstantiateMagicCallback
{
    // Directions in which the roots should go
    public float rotationWidthDegrees;
    public float nextSpawnTimeSec=10;
    public RootAiBuilder builder;

    private Vector2 _dirToMove;
    private Vector2 _leftVectorBoundary;
    private Vector2 _rightVectorBoundary;
    private TileManager _tileManager;
    private TileInfo _occupiedTile;
    private RootAi _nextChain;

    protected void Initialize(Vector2 dirToMove, float rotationWidth, TileInfo tile) {
        _dirToMove = dirToMove;
        rotationWidthDegrees = rotationWidth;
        _leftVectorBoundary = Quaternion.Euler(0, 0, -1*rotationWidth/2) * dirToMove;
        _rightVectorBoundary = Quaternion.Euler(0, 0, rotationWidth/2) * dirToMove;
        _occupiedTile = tile;
        _tileManager = GameManager.Instance.gameController.tileManager;
        StartCoroutine(SpawnNextChain());
    }

    // Start is called before the first frame update
    void Start()
    {
        builder = GetComponent<RootAiBuilder>();
        TiledGameObject tileObj = GetComponent<TiledGameObject>();
        // TODO, the passed in values are random
        Initialize(new Vector2(0, 1), 90, tileObj.originTile);
    }

    void Update() {

    }


    IEnumerator SpawnNextChain() {
        yield return new WaitForSeconds(nextSpawnTimeSec);
        // TODO We shouldn't look in all 4 directions, just between the vector boundaries
        Vector2Int curPositionInArray = _occupiedTile.positionInArray;
        Vector2Int nextPositionToSpawn;
        while(true) {
            int rand = NetworkingManager.RandomRangeUsingWorldSeed(0, 4);
            if(rand == 0) {
                // upleft
                if(_tileManager.IsSpawnableLocation(new Vector2Int(-1, 1) + curPositionInArray, new Vector2Int(1, 1))) {
                    nextPositionToSpawn = new Vector2Int(-1, 1) + curPositionInArray;
                    break;
                }
            } else if(rand == 1) {
                // upright
                if(_tileManager.IsSpawnableLocation(new Vector2Int(1, 1) + curPositionInArray, new Vector2Int(1, 1))) {
                    nextPositionToSpawn = new Vector2Int(1, 1) + curPositionInArray;
                    break;
                }
            } else if (rand == 2) {
                // downleft
                if(_tileManager.IsSpawnableLocation(new Vector2Int(-1, -1) + curPositionInArray, new Vector2Int(1, 1))) {
                    nextPositionToSpawn = new Vector2Int(-1, -1) + curPositionInArray;
                    break;
                } else {
                    Debug.Log("Can't Do it: " + (new Vector2Int(-1, -1) + curPositionInArray));
                }
            } else {
                // downright
                if(_tileManager.IsSpawnableLocation(new Vector2Int(1, -1) + curPositionInArray, new Vector2Int(1, 1))) {
                    nextPositionToSpawn = new Vector2Int(1, -1) + curPositionInArray;
                    break;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        TileInfo nextTile = _tileManager.GetTileInfoInArraySafe(nextPositionToSpawn.x, nextPositionToSpawn.y);
        builder.Initialize(_dirToMove, rotationWidthDegrees, nextTile);
        yield return null;
    }
}
