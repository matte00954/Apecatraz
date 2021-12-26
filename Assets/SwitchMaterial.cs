// Author: [full name here]
using UnityEngine;

public class SwitchMaterial : MonoBehaviour
{
    [SerializeField] private Material newMaterial;
    [SerializeField] private MeshRenderer affectedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!affectedObject.material.Equals(newMaterial))
            {
                affectedObject.material = newMaterial;
            }
        }
    }
}
