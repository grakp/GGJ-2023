using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;

public class RootAi : AiController // MonoBehaviourPun, IPunInstantiateMagicCallback
{
    // Directions in which the roots should go
    public float rotationWidthDegrees;
    public float nextSpawnTimeSec=10;
    public RootAiBuilder builder;
    public TiledPhotonView tiledPhotonView;

    private Vector2 _dirToMove;
    private Vector2 _leftVectorBoundary;
    private Vector2 _rightVectorBoundary;
    private TileManager _tileManager;
    private TileInfo _occupiedTile;
    private RootAi _nextChain;

    private PhotonView view;

    [Header("Enemy spawning")]
    public EnemySpawnParams spawnParams;
    public float enemySpawnInterval = 10.0f;
    private float currentSpawnTimer = 0.0f;

    public bool isMasterTile = false;

    public float randomSpawnVariance = 5.0f;
    private float currentRandomSpawnInterval;

    bool hasInitialized = false;

    protected void Initialize(Vector2 dirToMove, float rotationWidth, TileInfo tile) {

        hasInitialized = true;

        _dirToMove = dirToMove;
        rotationWidthDegrees = rotationWidth;
        _leftVectorBoundary = Quaternion.Euler(0, 0, -1*rotationWidth/2) * dirToMove;
        _rightVectorBoundary = Quaternion.Euler(0, 0, rotationWidth/2) * dirToMove;
        _occupiedTile = tile;
        transform.SetParent(GameManager.Instance.gameController.spawnedObjectParent);
        _tileManager = GameManager.Instance.gameController.tileManager;
        view = GetComponent<PhotonView>();

        currentRandomSpawnInterval = enemySpawnInterval + Random.Range(0.0f, randomSpawnVariance);

        if (view == null)
        {
            Debug.LogError("Cannot find view!");
        }

        if (tiledPhotonView.originTile == null)
        {
            Debug.LogError("Cannot find origin tile!: " + GameManager.Instance.gameController.tileManager.finishedWorldGeneration);
        }

         GameManager.Instance.gameController.tileManager.SetTileInUse(tiledPhotonView, tiledPhotonView.originTile);

        if (view.IsMine || GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            StartCoroutine(SpawnNextChain());
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        FixUpName();
        builder = GetComponent<RootAiBuilder>();
        // TODO, the passed in values are random
        if (GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            DoInitialize();
        }
    }

    private void DoInitialize()
    {
        TiledGameObject tileObj = GetComponent<TiledGameObject>();
        Initialize(new Vector2(0, 1), 90, tileObj.originTile);
    }

    void Update()
    {
        if (!hasInitialized)
        {
            return;
        }

        if (view.IsMine || GameManager.Instance.networkingManager.IsDebuggingMode)
        {
            if (isMasterTile)
            {
                currentSpawnTimer += Time.deltaTime;
                if (currentSpawnTimer >= currentRandomSpawnInterval)
                {
                    currentSpawnTimer = 0;
                    currentRandomSpawnInterval = enemySpawnInterval + Random.Range(0.0f, randomSpawnVariance);
                    int numEnemies = GameManager.Instance.gameController.GetNumEnemies();
                    int maxEnemies = GameManager.Instance.gameController.GetMaxEnemies();

                    if (numEnemies < maxEnemies)
                    {
                        SpawnEnemy();
                    }

                }
            }
        }

    }

    private void SpawnEnemy()
    {
        Vector2Int spawnLocation = GetRandomSpawnableRootLocation();
        if (spawnLocation == Vector2Int.zero)
        {
            return;
        }

        Vector3 instantiatePosition = GameManager.Instance.gameController.tileManager.GetWorldPositionOfTileInArray(spawnLocation);
        GameManager.Instance.gameController.SpawnEnemy(spawnParams.enemyPrefab, instantiatePosition);
    }

    IEnumerator SpawnNextChain() {
        yield return new WaitForSeconds(nextSpawnTimeSec);
        // TODO We shouldn't look in all 4 directions, just between the vector boundaries
        Vector2Int curPositionInArray = _occupiedTile.positionInArray;
        Vector2Int nextPositionToSpawn;
        while(true) {
            
            Vector2Int spawnedLocation = GetRandomSpawnableRootLocation();
            if (spawnedLocation != Vector2Int.zero)
            {
                nextPositionToSpawn = spawnedLocation;
                break;
            }
           
            yield return null;
        }
        TileInfo nextTile = _tileManager.GetTileInfoInArraySafe(nextPositionToSpawn.x, nextPositionToSpawn.y);
        builder.Initialize(_dirToMove, rotationWidthDegrees, nextTile);
        yield return null;

        if (isMasterTile)
        {
            StartCoroutine(SpawnNextChain());
        }
    }


    Vector2Int GetRandomSpawnableRootLocation()
    {
        Vector2Int curPositionInArray = _occupiedTile.positionInArray;
        Vector2Int nextPositionToSpawn;
        List<int> possibleValues = new List<int>{0, 1, 2, 3};
        possibleValues.Shuffle(null);
        for (int i = 0; i < possibleValues.Count; i++)
        {
            int rand = possibleValues[i];

            if(rand == 0) {
                // upleft
                if(_tileManager.IsSpawnableLocation(new Vector2Int(-1, 1) + curPositionInArray, new Vector2Int(1, 1)))
                    return new Vector2Int(-1, 1) + curPositionInArray;
            } else if(rand == 1) {
                // upright
                if(_tileManager.IsSpawnableLocation(new Vector2Int(1, 1) + curPositionInArray, new Vector2Int(1, 1)))
                    return new Vector2Int(1, 1) + curPositionInArray;
            } else if (rand == 2) {
                // downleft
                if(_tileManager.IsSpawnableLocation(new Vector2Int(-1, -1) + curPositionInArray, new Vector2Int(1, 1)))
                    return new Vector2Int(-1, -1) + curPositionInArray;
            } else {
                // downright
                if(_tileManager.IsSpawnableLocation(new Vector2Int(1, -1) + curPositionInArray, new Vector2Int(1, 1)))
                    return nextPositionToSpawn = new Vector2Int(1, -1) + curPositionInArray;
            }
        }

        return Vector2Int.zero;
    }

    // Want to spawn more generously so roots don't cover
    Vector2Int GetRandomSpawnableEnemyLocation()
    {
        Vector2Int curPositionInArray = _occupiedTile.positionInArray;
        Vector2Int offset = Vector2Int.zero;

        int rand = Random.Range(0, 4);
        if(rand == 0) {
            offset += Vector2Int.up;
        } else if(rand == 1) {
            offset += Vector2Int.down;
        } else if (rand == 2) {
            offset += Vector2Int.left;
        } else {
            offset += Vector2Int.right;
        }

        Vector2Int size = new Vector2Int(1, 2);
        Vector2Int currentPosition = curPositionInArray + offset;

        if (_tileManager.IsSpawnableLocation(currentPosition, size))
        {
            return currentPosition;
        }

        int lastRand = rand;
        while (currentPosition.x >= 0 && currentPosition.x < _tileManager.width && currentPosition.y >= 0 && currentPosition.y < _tileManager.height)
        {
            int newRand =  Random.Range(0, 3);
            // Do this so no chance of looping infinitely
            rand = (lastRand + newRand) % 4;

            if(rand == 0) {
                offset += Vector2Int.up;
            } else if(rand == 1) {
                offset += Vector2Int.down;
            } else if (rand == 2) {
                offset += Vector2Int.left;
            } else {
                offset += Vector2Int.right;
            }

            currentPosition = curPositionInArray + offset;
            if (_tileManager.IsSpawnableLocation(currentPosition, size))
            {
                return currentPosition;
            }

            lastRand = rand;
        }

        return Vector2Int.zero;
    }

    void InitializeRemote(int tileX, int tileY)
    {
        if (hasInitialized)
        {
            return;
        }

        if (GameManager.Instance.gameController.tileManager.finishedWorldGeneration)
        {
            tiledPhotonView.originTile = GameManager.Instance.gameController.tileManager.GetTileInfoInArraySafe(tileX, tileY);
            DoInitialize();
        }
        else
        {
            StartCoroutine(WaitForWorldGeneration(tileX, tileY));
        }
    }

    IEnumerator WaitForWorldGeneration(int tileX, int tileY)
    {
        while (!GameManager.Instance.gameController.tileManager.finishedWorldGeneration)
        {
            yield return null;
        }

        tiledPhotonView.originTile = GameManager.Instance.gameController.tileManager.GetTileInfoInArraySafe(tileX, tileY);
        DoInitialize();
    }

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] customData = info.photonView.InstantiationData;
        int tileX = (int)customData[0];
        int tileY = (int)customData[1];

        InitializeRemote(tileX, tileY);
    }

}
