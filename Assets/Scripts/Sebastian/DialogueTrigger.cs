// Author: Sebastian Kl�tz
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    private const string PlayerTagName = "Player";

    [SerializeField] private Dialogue dialogue;
    [SerializeField] private GameObject canReadUI;

    public void TriggerDialogue() => FindObjectOfType<DialogueManager>().StartDialogue(dialogue);

    private void Start() => canReadUI.SetActive(false);

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag(PlayerTagName) && Input.GetButtonDown("Fire4") && !DialogueManager.IsActive)
            TriggerDialogue();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(PlayerTagName))
            canReadUI.SetActive(true); 
            canReadUI.transform.position = this.transform.position + Vector3.up;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(PlayerTagName))
            canReadUI.SetActive(false);
    }
}
