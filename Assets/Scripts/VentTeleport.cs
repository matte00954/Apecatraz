using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VentTeleport : MonoBehaviour
{
    [SerializeField]
    private Transform exitLocation;
    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<CharacterController>().enabled = false;
            other.transform.position = exitLocation.position;
            other.GetComponent<CharacterController>().enabled = true;
        }
    }
}
