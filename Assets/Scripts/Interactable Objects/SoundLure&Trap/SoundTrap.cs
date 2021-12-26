// Author: William Örnquist
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class SoundTrap : MonoBehaviour
{
    private const float DefaultCooldown = 2f;
    private const float TimerResetValue = 0f;
    private const string PlayerLayerName = "Player";

#pragma warning disable IDE0044 // Add readonly modifier
    [SerializeField, Range(0.1f, 5f)] private float triggerCooldown = DefaultCooldown;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private UnityEvent onCollision;
#pragma warning restore IDE0044 // Add readonly modifier

    private AudioSource audioSource;
    
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
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer(PlayerLayerName)))
        {
            Debug.Log("SoundTrap triggered");
            if (cooldownTimer >= triggerCooldown)
            {
                onCollision.Invoke(); // Activates a method in the 'SoundLure'-script that emits the lure trigger collider for a brief second.
                cooldownTimer = TimerResetValue;
                audioSource.PlayOneShot(collisionSound);
            }
        }
    }
#pragma warning restore IDE0051 // Remove unused private members
}
