// Author: [full name here]
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] private Boid prefab;
    [SerializeField] private float spawnRadius = 10;
    [SerializeField] private int spawnCount = 10;

    private void Awake()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + (Random.insideUnitSphere * spawnRadius);
            Boid boid = Instantiate(prefab);
            boid.transform.position = pos;
            boid.transform.forward = Random.insideUnitSphere;
        }
    }
}