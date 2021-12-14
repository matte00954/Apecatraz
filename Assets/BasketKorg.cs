using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketKorg : MonoBehaviour
{
    [SerializeField] private AudioClip yay;
    private bool hasScored = false;
    [SerializeField] private GameObject bananaPrefab;
    [SerializeField] private GameObject confettiParticle;

    private void Start()
    {
        confettiParticle.SetActive(false);
        bananaPrefab.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball") && hasScored == false)
        {
            SpawnBanana();
            hasScored = true;
        }
    }

    private void SpawnBanana()
    {
        gameObject.GetComponent<AudioSource>().PlayOneShot(yay, 1f);
        confettiParticle.SetActive(true);
        bananaPrefab.SetActive(true);
        //throw new NotImplementedException();
    }
}
