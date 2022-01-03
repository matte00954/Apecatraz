using UnityEngine;
using TMPro;

public class TutorialText : MonoBehaviour
{
    [SerializeField] string str;

    [SerializeField] TextMeshProUGUI textMeshPro;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            textMeshPro.text = str;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            textMeshPro.text = string.Empty;
        }
    }
}
