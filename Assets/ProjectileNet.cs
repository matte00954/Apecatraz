using UnityEngine;

public class ProjectileNet : MonoBehaviour
{
    [SerializeField] private LayerMask raycastPlayerMask;
    [SerializeField, Range(1f, 50f)] private float speed = 5f;
    [SerializeField, Range(1f, 10f)] private float maxLifetime = 5f;

    private float lifeTimer = 0f;
    private bool isActive = true;

    void Update()
    {
        if (isActive)
        {
            RayCheck();
            transform.position += transform.forward * speed * Time.deltaTime;

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifetime)
                Destroy(this.gameObject);
        }
    }

    private void RayCheck()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit raycastHit, speed * Time.deltaTime, raycastPlayerMask) && isActive)
        {
            isActive = false;
            if (raycastHit.collider.gameObject.CompareTag("Player"))
            {
                //Kill monke. mmmm, monke.
                Destroy(this.gameObject);
            }
            Destroy(this.gameObject, 2f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Collision"))
        {
            isActive = false;
            Destroy(this.gameObject, 2f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * speed * Time.deltaTime);
    }
}
