//Author: Hanna Hellberg
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineChanger : MonoBehaviour
{
    public Material outlineMat;

    public void HandleInputData(int val)
    {
        if (val == 0)
        {
            outlineMat.SetFloat("Alpha", 1);
        }
        if (val == 1)
        {
            outlineMat.SetFloat("Alpha", 0.02f);
        }
        if (val == 2)
        {
            outlineMat.SetFloat("Alpha", 0);
        }
    }    
}
