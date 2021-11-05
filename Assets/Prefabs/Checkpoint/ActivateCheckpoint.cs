using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCheckpoint : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private GameObject spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.SetCurrentCheckpoint(spawnPoint);
        }
    }
}
