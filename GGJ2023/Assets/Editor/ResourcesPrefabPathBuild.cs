#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class ResourcesPrefabPathBuild : IPreprocessBuildWithReport
{

    public int callbackOrder{ get { return 0; }}
    

    public void OnPreprocessBuild(BuildReport report)
    {
        NetworkingSingleton.PopulateNetworkedPrefabs();
    }
}
#endif