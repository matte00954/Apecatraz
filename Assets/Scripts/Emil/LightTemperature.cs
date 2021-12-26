// Author: Emil Moqvist
using UnityEngine;

public class LightTemperature : MonoBehaviour
{
    private readonly float transitionValue = 1.0f;
    private Light lightComp;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player") 
            lightComp.color = Color.red;
    }

    private void Start() => lightComp = GetComponent<Light>();

    private void Update()
    {
        lightComp.color = Color.Lerp(Color.yellow, Color.red, Mathf.PingPong(Time.time, transitionValue) / transitionValue);
    }
}
