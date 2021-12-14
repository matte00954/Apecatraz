using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue dialogue;

    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);

    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q) && !DialogueManager.isActive)
        {
            TriggerDialogue();
        }
    }

}
