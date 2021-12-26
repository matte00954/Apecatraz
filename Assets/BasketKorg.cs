// Author: [full name here]
using UnityEngine;

public class BasketKorg : MonoBehaviour
{
    [SerializeField] private AudioClip yayClip;
    [SerializeField] private GameObject bananaPrefab;
    [SerializeField] private GameObject confettiParticle;

    private bool hasScored = false;

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
        gameObject.GetComponent<AudioSource>().PlayOneShot(yayClip, 1f);
        confettiParticle.SetActive(true);
        bananaPrefab.SetActive(true);
        ////throw new NotImplementedException();
    }
}
