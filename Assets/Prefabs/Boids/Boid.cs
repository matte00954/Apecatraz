using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    BoidSettings settings;
    public Vector3 position;
    public Vector3 forward;
    public Vector3 velocity;
    Transform target;
    Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
    }
    public void Initialize(BoidSettings settings, Transform target)
    {
        this.settings = settings;
        this.target = target;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }
}
