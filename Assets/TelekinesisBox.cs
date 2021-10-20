using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisBox : MonoBehaviour
{
    public GameObject PickupMessagePanel;

    private void Start() {
        PickupMessagePanel = GameObject.FindGameObjectWithTag("PickupMessagePanel");
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PickupMessagePanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.tag == "Player")
        {
            PickupMessagePanel.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown("f") && other.tag == "Player")
        {
            Debug.Log("Talking");
        }
    }
}
