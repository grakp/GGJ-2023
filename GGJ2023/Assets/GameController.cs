using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform spawnedObjectParent;
    
    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance.gameController = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
