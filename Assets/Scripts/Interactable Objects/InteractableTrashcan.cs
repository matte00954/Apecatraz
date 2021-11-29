using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableTrashcan : MonoBehaviour
{
    [SerializeField]
    private GameObject trashCanText;
    private bool IsHiding = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            trashCanText.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q) && !IsHiding)
        {
            SetActiveAllChildren(other.transform, false);
            IsHiding = true;
        }
        else if (other.gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q))
        {
            SetActiveAllChildren(other.transform, true);
            IsHiding = false;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            trashCanText.SetActive(false);
            other.gameObject.SetActive(true);
        }
    }

    private void SetActiveAllChildren(Transform transform, bool value)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(value);

            SetActiveAllChildren(child, value);
        }
    }
}

