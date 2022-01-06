using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private Light light;
    [SerializeField, Range(0, 10)] private float flickTime;

    private float flickTimer;

    private void Update()
    {
        if (flickTimer <= 0f)
        {
            light.enabled = !light.enabled;
            flickTimer = flickTime;
        }
        else
            flickTimer -= Time.deltaTime;
    }
}
