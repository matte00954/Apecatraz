// Author: [full name here]
using UnityEngine;

public class MonitorTrigger : MonoBehaviour
{
    [SerializeField] private GameObject camera1;
    [SerializeField] private GameObject camera2;

    private bool monitorIsOn;

    private void Start()
    {
        camera1.SetActive(false);
        camera2.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !monitorIsOn)
        {
            camera1.SetActive(true);
            camera2.SetActive(true);
            monitorIsOn = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && monitorIsOn)
        {
            monitorIsOn = false;
            camera1.SetActive(false);
            camera2.SetActive(false);
        }
    }
}
