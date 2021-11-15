using UnityEngine;
using UnityEngine.Events;

public class VentTeleport : MonoBehaviour
{
    [SerializeField]
    private Transform exitLocation;
    [SerializeField]
    private UnityEvent onEnter;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<CharacterController>().enabled = false;
            other.transform.position = exitLocation.position;
            other.GetComponent<CharacterController>().enabled = true;
            onEnter.Invoke();
        }
    }
}
