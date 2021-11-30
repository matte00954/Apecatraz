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
            other.transform.position = exitLocation.position;
            onEnter.Invoke();
        }
    }
}
