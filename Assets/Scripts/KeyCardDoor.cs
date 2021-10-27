using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCardDoor : MonoBehaviour
{

    [SerializeField]
    private string Color;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            if(other.GetComponent<KeyChain>().CheckIfKeycardIsInKeychain(Color))
                transform.parent.gameObject.SetActive(false);
        }
    }
}
