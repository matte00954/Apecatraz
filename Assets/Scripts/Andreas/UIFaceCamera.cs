// Author: Andreas Scherman
using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        if (cameraObject == null)
            cameraObject = GameObject.FindWithTag("MainCamera");
    }

    private void Update()
    {
        if (canvas.enabled)
            canvas.transform.LookAt(cameraObject.transform);
    }
}
