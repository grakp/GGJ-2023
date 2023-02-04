using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class NetworkedPrefab
{
    public GameObject prefab;
    public string path;

    public NetworkedPrefab(GameObject obj, string path)
    {
        prefab = obj;
        this.path = ReturnPrefabPathModified(path);
        // Assets/resources/File.prefab
        // Resource/File
    }

    private string ReturnPrefabPathModified(string path)
    {
        int extensionLength = System.IO.Path.GetExtension(path).Length;
        int additionalLength = 10;
        int startIndex = path.ToLower().IndexOf("resources");

        if (startIndex == -1)
        {
            return string.Empty;
        }
        else
        {
            return path.Substring(startIndex + additionalLength, path.Length - (additionalLength + startIndex + extensionLength));
        }
    }
};

[CreateAssetMenu(menuName = "Singletons/NetworkingSingleton")]
public class NetworkingSingleton : SingletonScriptableObject<NetworkingSingleton>
{
    public List<NetworkedPrefab> networkedPrefabs;

    public static GameObject NetworkInstantiate(GameObject obj, Vector3 position, Quaternion rotation)
    {
        foreach (NetworkedPrefab networkedPrefab in NetworkingSingleton.Instance.networkedPrefabs)
        {
            if (networkedPrefab.prefab == obj)
            {
                if (networkedPrefab.path != string.Empty)
                {
                    GameObject result = PhotonNetwork.Instantiate(networkedPrefab.path, position, rotation);
                }
                else
                {
                    Debug.LogError("Path is empty for gameobject name: " + networkedPrefab.prefab);
                    return null;
                }
            }
        }

        return null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void PopulateNetworkedPrefabs()
    {
#if UNITY_EDITOR
        NetworkingSingleton.Instance.networkedPrefabs.Clear();
        GameObject[] results = Resources.LoadAll<GameObject>("");
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].GetComponent<PhotonView>() != null)
            {
                string path = AssetDatabase.GetAssetPath(results[i]);
                NetworkingSingleton.Instance.networkedPrefabs.Add(new NetworkedPrefab(results[i], path));
            }
        }
#endif
    }

}
