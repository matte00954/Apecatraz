using UnityEngine;

public class MagnifyObject : MonoBehaviour
{
    Renderer rend;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(transform.position);
        screenPoint.x = screenPoint.x / Screen.width;
        screenPoint.y = screenPoint.y / Screen.height;
        rend.material.SetVector("ObjScreenPos", screenPoint);
    }
}
