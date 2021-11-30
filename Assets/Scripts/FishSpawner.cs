
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;


    void Awake()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Boid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;
        }
    }
}
