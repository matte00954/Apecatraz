//Author: William Örnquist
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SoundLure : MonoBehaviour
{
    private Collider lureCollider;
    [SerializeField, Range(0.1f, 3f), Tooltip("How many seconds the luring collider lasts upon activation.")]
    private float activityTime = 1f;

    private float activityTimer;

    private void Start()
    {
        lureCollider = GetComponent<Collider>();
        activityTimer = activityTime;
        lureCollider.enabled = false;
    }

    void Update()
    {
        if (lureCollider.enabled && activityTimer < activityTime)
            activityTimer += Time.deltaTime;
        else if (lureCollider.enabled && activityTimer >= activityTime)
        {
            lureCollider.enabled = false;
        }
    }

    public void ActivateLure()
    {
        if (!lureCollider.enabled)
        {
            lureCollider.enabled = true;
            activityTimer = 0f;
        }
    }
}