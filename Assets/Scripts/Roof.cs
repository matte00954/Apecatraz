// Author: [full name here]
using UnityEngine;

public class Roof : MonoBehaviour
{
    [SerializeField] private GameObject roof;

    private void Awake() => roof.GetComponent<MeshRenderer>().enabled = true;
}
