using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigRedButton : MonoBehaviour
{
    [SerializeField] private GameObject interactableObjectsText;

    private bool canPress = false;

    private void Start()
    {
        if (interactableObjectsText.activeInHierarchy) interactableObjectsText.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (canPress == true)
            {
                playExplosionAnimation();
            }
        }
    }

    private void playExplosionAnimation()
    {
        throw new NotImplementedException();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BigRedButton"))
        {
            interactableObjectsText.SetActive(true);
            canPress = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BigRedButton"))
        {
            interactableObjectsText.SetActive(false);
            canPress = false;
        }

    }
}
