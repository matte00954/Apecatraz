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
            other.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            onEnter.Invoke();
        }
    }
}
