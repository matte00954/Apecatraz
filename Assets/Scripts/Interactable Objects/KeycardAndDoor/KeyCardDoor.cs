using UnityEngine;
using UnityEngine.Events;

public class KeyCardDoor : MonoBehaviour
{

    [SerializeField]
    private string Color;
    [SerializeField]
    private UnityEvent onUnlock;

    private void OnTriggerEnter(Collider other) 
    {
        if(other.CompareTag("Player"))
        {
            if(other.GetComponent<KeyChain>().CheckIfKeycardIsInKeychain(Color))
            {
                onUnlock.Invoke();
                transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
