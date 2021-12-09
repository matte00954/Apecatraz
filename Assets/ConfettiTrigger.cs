using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiTrigger : MonoBehaviour
{

    public GameObject confettiFX;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Ball"))
        {
            GameObject particle = Instantiate(confettiFX, this.transform.position, Quaternion.identity);
            particle.GetComponent<ParticleSystem>().Play();
        }
        
    }
}
