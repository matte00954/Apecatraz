// Author: Emil Moqvist
using UnityEngine;

public class MagnifyObject : MonoBehaviour
{
    private Renderer rend;
    private Camera cam;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        cam = Camera.main;
    }

    private void Update()
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(transform.position);
        screenPoint.x /= Screen.width;
        screenPoint.y /= Screen.height;
        rend.material.SetVector("ObjScreenPos", screenPoint);
    }
}
