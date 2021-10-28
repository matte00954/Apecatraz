using UnityEngine;

public class Roof : MonoBehaviour
{
    [SerializeField] private GameObject roof;

    // Start is called before the first frame update
    void Awake()
    {
        roof.GetComponent<MeshRenderer>().enabled = true;
    }
}
