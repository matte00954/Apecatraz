using UnityEngine;
public class JSONReader : MonoBehaviour
{
    public TextAsset jsonFile;

    void Start()
    {
        EnemyVariables EnemyMovementInJson = JsonUtility.FromJson<EnemyVariables>(jsonFile.text);
        Debug.Log("Loaded player settings ::::");
        Debug.Log("Loaded Patrolspeed : " + EnemyMovementInJson.patrolSpeed);
        Debug.Log("Loaded alertSpeed : " + EnemyMovementInJson.alertSpeed);
        Debug.Log("Loaded DumbstruckTime: " + EnemyMovementInJson.dumbstruckTime);
    }
}