using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class DebugTestNetworkPaths : MonoBehaviour
{

    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        string displayText = NetworkingSingleton.Instance.networkedPrefabs.Count.ToString();

        if ( NetworkingSingleton.Instance.networkedPrefabs.Count > 0 )
        {
            displayText += " " +  NetworkingSingleton.Instance.networkedPrefabs[0].path;
        }

        text.text = displayText;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
