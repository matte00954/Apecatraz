using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFaceCamera : MonoBehaviour
{
    public GameObject camera;
    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        if (camera == null)
        {
            camera = GameObject.FindWithTag("MainCamera");

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canvas.enabled)
        {
            canvas.transform.LookAt(camera.transform);
        }
    }
}
