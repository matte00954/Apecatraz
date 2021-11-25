// Author: Emil Moqvist
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSwap : MonoBehaviour
{
    private Color[] colors;

    public Material mat1;
    public Material mat2;

    //Lägg till getcomponent carriedobject set materialcolor = rend color.

    // Start is called before the first frame update
    void Start()
    {
        colors = new Color[5];

        colors[0] = Color.blue;
        colors[1] = Color.red;
        colors[2] = Color.green;
        colors[3] = Color.yellow;
        colors[4] = Color.magenta;
        AssignColors(0);

    }

    // Update is called once per frame
  /*  void Update()
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
        
    }*/

    public void AssignColors(int colorNumber)
    {
        mat1.SetColor("GradientNoiseColor", colors[colorNumber]);
        mat2.SetColor("GradientNoiseColor", colors[colorNumber]);
    }

}
