using UnityEngine;

public class SoundLure : MonoBehaviour
{
    [SerializeField] private Collider lureCollider;
    [SerializeField, Range(0.1f, 3f)] private float activityTime = 1f;
    private float timer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && !lureCollider.enabled)
        {
            lureCollider.enabled = true;
            timer = 0f;
        }

        if (lureCollider.enabled && timer < activityTime)
            timer += Time.deltaTime;
        else if (lureCollider.enabled && timer >= activityTime)
            lureCollider.enabled = false;
    }
}
