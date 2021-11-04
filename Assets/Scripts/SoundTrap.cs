using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class SoundTrap : MonoBehaviour
{
    private const float DEFAULT_COOLDOWN = 2f;
    private const float TIMER_RESET = 0f;
#pragma warning disable IDE0044 // Add readonly modifier
    [SerializeField, Range(0.1f, 5f)] private float triggerCooldown = DEFAULT_COOLDOWN;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private UnityEvent onCollision;
#pragma warning restore IDE0044 // Add readonly modifier

    private AudioSource audioSource;
    private readonly string playerName = "CanBeCarried";
    private float cooldownTimer;

#pragma warning disable IDE0051 // Remove unused private members
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        cooldownTimer = triggerCooldown;
    }

    private void Update()
    {
        if (cooldownTimer < triggerCooldown)
            cooldownTimer += Time.deltaTime;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer(playerName)))
        {
            Debug.Log("Collision Success");
            if (cooldownTimer >= triggerCooldown)
            {
                onCollision.Invoke();
                cooldownTimer = TIMER_RESET;
                audioSource.PlayOneShot(collisionSound);
            }
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}
