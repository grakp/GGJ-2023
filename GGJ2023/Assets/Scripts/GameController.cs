using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Transform spawnedObjectParent;

    public UIController uiController;
    
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
