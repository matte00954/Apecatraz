using UnityEngine;

public class ProjectileNet : MonoBehaviour
{
    [SerializeField, Tooltip("The layers the projectile will collide with (exluding 'Player').")]
    private LayerMask collisionMask;
    [SerializeField, Range(1f, 50f)] private float speed = 5f;
    [SerializeField, Range(1f, 10f)] private float maxLifetime = 5f;

    private readonly string playerLayerName = "Player";

    private float lifeTimer = 0f;
    private bool isActive = true;

    void Update()
    {
        if (isActive)
        {
            transform.position += transform.forward * speed * Time.deltaTime;

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifetime)
                Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collisionMask == (collisionMask | 1 << other.gameObject.layer))
        {
            isActive = false;
            Destroy(this.gameObject, 2f);
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer(playerLayerName) && isActive)
        {
            ResetScene.RestartScene();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * speed * Time.deltaTime);
    }
}
