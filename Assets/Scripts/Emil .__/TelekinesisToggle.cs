using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisToggle : MonoBehaviour
{
    public Material[] materials;
    public int x;
    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        x = 0;
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[x];
    }

    // Update is called once per frame
    void Update()
    {
        rend.sharedMaterial = materials[x];

        if (Input.GetButtonDown("Fire1"))
        {
            x++;
           // ToggleTelekinesis();
        }
        if (Input.GetButtonUp("Fire1"))
        {
            x--;
        }
    }

   /* public void ToggleTelekinesisOn()
    {
        if (x < 1)
        {
            x++;
        }
        else
        {
            x = 0;
        }
    } */

   
}
