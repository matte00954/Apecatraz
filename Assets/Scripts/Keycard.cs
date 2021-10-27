using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keycard : MonoBehaviour
{
    [SerializeField]
    private string Color;
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<KeyChain>().AddKeyCard(Color);
            gameObject.SetActive(false);
        }
    }
}
