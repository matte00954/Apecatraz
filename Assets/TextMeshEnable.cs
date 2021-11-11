using UnityEngine;

public class TextMeshEnable : MonoBehaviour
{
    [SerializeField] private TextMesh textToEnable;
    private string textInEditor;

    private void Start()
    {
        textInEditor = textToEnable.text;
        textToEnable.text = "";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            textToEnable.text = textInEditor;
        }
    }
}
