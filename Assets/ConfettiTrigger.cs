// Author: [full name here]
using System.Collections;
using UnityEngine;

public class ConfettiTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip celebrateClip;
    [SerializeField] private GameObject particleSystemObject; // TODO: refer ParticleSystem instead?

    private AudioSource audioSource;

    private bool isPlaying = false;

    private void Start()
    {
        particleSystemObject.GetComponent<ParticleSystem>().Stop();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (!isPlaying)
                StartCoroutine(Wait());
            
            ////GameObject particle = Instantiate(confettiFX, this.transform.position, Quaternion.identity);
            ////particle.GetComponent<ParticleSystem>().Play();
        }
    }

    private IEnumerator Wait()
    {
        particleSystemObject.GetComponent<ParticleSystem>().Play();
        audioSource.PlayOneShot(celebrateClip, 0.7f);
        isPlaying = true;

        yield return new WaitForSeconds(4);

        particleSystemObject.GetComponent<ParticleSystem>().Stop();
        isPlaying = false;
    }
}
