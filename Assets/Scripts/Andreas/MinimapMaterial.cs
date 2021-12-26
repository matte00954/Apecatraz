// Author: Andreas Scherman
using UnityEngine;

public class MinimapMaterial : MonoBehaviour
{
    [SerializeField] private Shader shader;
    [SerializeField] private Camera minimapCamera;

    private void Start()
    {
        ////camera = GetComponent<Camera>();
        minimapCamera.SetReplacementShader(shader, "HDLitShader");
    }
}
