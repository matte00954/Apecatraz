// Author: [full name here]
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    private string path;

    ////public TextAsset jsonFile;

    private void Start()
    {
        path = Application.streamingAssetsPath + "/EnemyVariables.json";
        EnemyVariables enemyMovementInJson = JsonUtility.FromJson<EnemyVariables>(path);
        Debug.Log("Loaded player settings ::::");
        Debug.Log("Loaded Patrolspeed : " + enemyMovementInJson.patrolSpeed);
        Debug.Log("Loaded alertSpeed : " + enemyMovementInJson.alertSpeed);
        Debug.Log("Loaded DumbstruckTime: " + enemyMovementInJson.dumbstruckTime);
    }
}