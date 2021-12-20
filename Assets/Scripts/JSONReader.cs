using UnityEngine;
public class JSONReader : MonoBehaviour
{
    string path;

    //public TextAsset jsonFile;

    void Start()
    {
        path = Application.streamingAssetsPath + "/EnemyVariables.json";
        EnemyVariables EnemyMovementInJson = JsonUtility.FromJson<EnemyVariables>(path);
        Debug.Log("Loaded player settings ::::");
        Debug.Log("Loaded Patrolspeed : " + EnemyMovementInJson.patrolSpeed);
        Debug.Log("Loaded alertSpeed : " + EnemyMovementInJson.alertSpeed);
        Debug.Log("Loaded DumbstruckTime: " + EnemyMovementInJson.dumbstruckTime);
    }
}