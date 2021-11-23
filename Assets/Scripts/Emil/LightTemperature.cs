using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTemperature : MonoBehaviour
{
    float transition = 1.0f;
    Color color0 = Color.yellow;
    Color color1 = Color.red;

    Light light;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        float t = Mathf.PingPong(Time.time, transition) / transition;
        light.color = Color.Lerp(color0, color1, t);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player")
        {
            light.color = color1;
        }
    }

}
