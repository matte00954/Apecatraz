using UnityEngine;

public class PlayerHeadCollider : MonoBehaviour
{
    [SerializeField] private GameObject head;
    [SerializeField] private LayerMask groundMask;
    private Collider headCollider;

    private void Start()
    {
        headCollider = GetComponent<Collider>();
    }
    private void Update()
    {
        if (headCollider.enabled == true && Physics.Raycast(head.transform.position, Vector3.down, 0.2f, groundMask)
            && (headCollider.enabled == true && Physics.Raycast(head.transform.position, transform.forward, 1f, groundMask)))
        {
            headCollider.enabled = false;
        }
        else if (headCollider.enabled == false)
        {
            headCollider.enabled = true;
        }
    }
}
