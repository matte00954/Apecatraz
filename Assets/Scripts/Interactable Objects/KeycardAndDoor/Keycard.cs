using UnityEngine;
using UnityEngine.Events;

public class Keycard : MonoBehaviour
{
    [SerializeField] private string color;
    [SerializeField] private UnityEvent onPickup;
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player"))
        {
            gameObject.GetComponentInParent<AudioSource>().PlayOneShot(pickupSound, 1f);
            onPickup.Invoke();
            other.GetComponent<KeyChain>().AddKeyCard(color);
            gameObject.SetActive(false);
        }
    }
}
