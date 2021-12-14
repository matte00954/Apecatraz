using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiTrigger : MonoBehaviour
{

    AudioSource audioSource;

    public AudioClip celebrate;
    public GameObject confettiFX;

    private bool isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        confettiFX.GetComponent<ParticleSystem>().Stop();
        audioSource = GetComponent<AudioSource>();
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Ball"))
        {
            if (!isPlaying)
            {
                StartCoroutine(waiting());
            }    
            
            //GameObject particle = Instantiate(confettiFX, this.transform.position, Quaternion.identity);
            //particle.GetComponent<ParticleSystem>().Play();
        }
        
    }

    IEnumerator waiting()
    {
        
        confettiFX.GetComponent<ParticleSystem>().Play();
        audioSource.PlayOneShot(celebrate, 0.7f);
        isPlaying = true;

        yield return new WaitForSeconds(4);

        confettiFX.GetComponent<ParticleSystem>().Stop();
        isPlaying = false;

    }

}
