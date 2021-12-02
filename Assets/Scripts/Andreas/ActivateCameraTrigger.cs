using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCameraTrigger : MonoBehaviour
{
    [SerializeField]
    private Cinemachine.CinemachineVirtualCamera cinemachine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            cinemachine.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            cinemachine.enabled = false;
    }
}
