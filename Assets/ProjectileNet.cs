using UnityEngine;

public class ProjectileNet : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private LayerMask collisionMask;

    private Vector3 direction;
    private float lifeTimer;
    private bool isActive = true;

    private void Awake()
    {

    }

    void Update()
    {
        
    }
}
