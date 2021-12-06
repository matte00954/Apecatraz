using UnityEngine;
using UnityEngine.Events;

public class KeyCardDoor : MonoBehaviour
{

    [SerializeField]
    private string Color;
    [SerializeField]
    private UnityEvent onUnlock;

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Collider door;

    private bool opened = false;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            if(other.GetComponent<KeyChain>().CheckIfKeycardIsInKeychain(Color) && opened == false)
            {
                onUnlock.Invoke();
                animator.SetTrigger("Open");
                door.enabled = false;
                opened = true;
            }
        }
    }
}
