using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleOutline : MonoBehaviour
{
    public GameObject[] outlines;
    private bool isEnabled;

    // Start is called before the first frame update
    void Start()
    {
        turnOffOutlines();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isEnabled)
        {
            isEnabled = true;            
            foreach (GameObject outline in outlines)
            {
                outline.SetActive(true);                
            }           
        }
        else if(Input.GetKeyDown(KeyCode.T) && isEnabled)
        {            
            turnOffOutlines();
        }
    }

    private void turnOffOutlines()
    {
        isEnabled = false;
        foreach (GameObject outline in outlines)
        {
            outline.SetActive(false);
        }        
    }
}
