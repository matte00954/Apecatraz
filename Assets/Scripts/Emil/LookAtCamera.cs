// Author: Emil Moqvist
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera cam;

    private void Start() => cam = Camera.main;

    private void Update()
    {
        transform.forward = cam.transform.position - transform.position;
    }
}
