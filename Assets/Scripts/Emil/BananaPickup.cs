using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaPickup : MonoBehaviour
{

    private GameObject parentObj;

    private float t;

    // Start is called before the first frame update
    void Start()
    {
        parentObj = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 90 * Time.deltaTime, 0);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(parentObj);
        }
    }

}
