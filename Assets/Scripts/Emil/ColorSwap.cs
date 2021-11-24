// Author: Emil Moqvist
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSwap : MonoBehaviour
{
    private Color[] colors;
    private int index;

    public Material mat1;
    public Material mat2;

    //Lägg till getcomponent carriedobject set materialcolor = rend color.

    // Start is called before the first frame update
    void Start()
    {

        index = 0;
        colors = new Color[5];

        colors[0] = Color.blue;
        colors[1] = Color.yellow;
        colors[2] = Color.red;
        colors[3] = Color.green;
        colors[4] = Color.magenta;
        AssignColors();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (index < 4)
            {
                index++;
                AssignColors();
                print("T IS PRESS " + index);
            }
            else
            {
                index = 0;
                AssignColors();
                print("T IS PRESS " + index);
            }
        }
        
    }

    private void AssignColors()
    {
        mat1.SetColor("GradientNoiseColor", colors[index]);
        mat2.SetColor("GradientNoiseColor", colors[index]);
    }

}
