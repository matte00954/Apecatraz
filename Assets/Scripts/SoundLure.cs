using UnityEngine;

public class SoundLure : MonoBehaviour
{
    [Header("(Press Z to test activation in game)")]
    [Space(20f)]
    [SerializeField] private Collider lureCollider;
    [SerializeField, Range(0.1f, 3f), Tooltip("How long the collider stays active upon activation.")]
    private float activityTime = 1f;

    private float timer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            ActivateLure();

        if (lureCollider.enabled && timer < activityTime)
            timer += Time.deltaTime;
        else if (lureCollider.enabled && timer >= activityTime)
            lureCollider.enabled = false;
    }

    public void ActivateLure()
    {
        if (!lureCollider.enabled)
        {
            lureCollider.enabled = true;
            timer = 0f;
        }
    }
}
