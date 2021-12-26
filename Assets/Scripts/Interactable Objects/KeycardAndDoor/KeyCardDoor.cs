using UnityEngine;
using UnityEngine.Events;

public class KeyCardDoor : MonoBehaviour
{
    [SerializeField] private string color;
    [SerializeField] private UnityEvent onUnlock;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider door;
    [SerializeField] private AudioSource audioSource;

    private bool opened = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<KeyChain>().IsKeycardInKeychain(color) && opened == false)
            {
                audioSource.Play();
                onUnlock.Invoke();
                animator.SetTrigger("Open");
                ////door.enabled = false;
                opened = true;
                Debug.Log("opened door : " + gameObject.name);
            }
        }
    }
}
