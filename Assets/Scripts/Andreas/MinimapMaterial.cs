using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapMaterial : MonoBehaviour
{
    public Shader shader;
    public Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        //camera = GetComponent<Camera>();
        camera.SetReplacementShader(shader, "HDLitShader");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
