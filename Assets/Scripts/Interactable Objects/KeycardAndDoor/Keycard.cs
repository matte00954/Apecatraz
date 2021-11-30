using UnityEngine;
using UnityEngine.Events;

public class Keycard : MonoBehaviour
{
    [SerializeField]
    private string Color;
    [SerializeField]
    private UnityEvent onPickup;

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player"))
        {
            onPickup.Invoke();
            other.GetComponent<KeyChain>().AddKeyCard(Color);
            gameObject.SetActive(false);
        }
    }
}
