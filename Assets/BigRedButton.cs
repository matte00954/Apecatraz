using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigRedButton : MonoBehaviour
{
    [SerializeField] private GameObject cutsceneCamera;

    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject interactableObjectsText;

    private float explosiontimer = 5f;

    private bool canPress = false;

    [SerializeField] private GameObject boom;
    private bool prepareToExplode = false;

    private void Start()
    {
        if (interactableObjectsText.activeInHierarchy) interactableObjectsText.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (canPress == true && prepareToExplode == false)
            {
                cutsceneCamera.SetActive(true);
                playerCamera.SetActive(false);
                prepareToExplode = true;
            }
        }
        if (prepareToExplode == true)
        {
            explosiontimer -= Time.deltaTime;
        }
        if (explosiontimer < 0)
        {
            playExplosionAnimation();
        }
    }

    private void playExplosionAnimation()
    {

        boom.SetActive(true);
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
