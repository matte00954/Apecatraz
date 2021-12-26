using UnityEngine;

public class TelekinesisBox : MonoBehaviour
{
    [SerializeField] private GameObject pickupMessagePanel;

    private void Start() => pickupMessagePanel = GameObject.FindGameObjectWithTag("PickupMessagePanel");
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            pickupMessagePanel.SetActive(true);
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player"))
            pickupMessagePanel.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown("f") && other.CompareTag("Player"))
            Debug.Log("Talking");
    }
}
