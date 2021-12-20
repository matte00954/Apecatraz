using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleOutline : MonoBehaviour
{
    public GameObject[] outlines;
    public GameObject[] highlights;

    private bool outlineIsEnabled;
    private bool highlightIsEnabled;

    // Start is called before the first frame update
    void Start()
    {
        turnOffOutlines();
        turnOffHighlights();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !outlineIsEnabled)
        {
            outlineIsEnabled = true;            
            foreach (GameObject outline in outlines)
            {
                outline.SetActive(true);                
            } 
                       
        }
        else if(Input.GetKeyDown(KeyCode.T) && outlineIsEnabled)
        {            
            turnOffOutlines();            
        }

        if (Input.GetKeyDown(KeyCode.G) && !highlightIsEnabled)
        {
            highlightIsEnabled = true;
            foreach (GameObject highlight in highlights)
            {
                highlight.SetActive(true);
            }            
        }
        else if (Input.GetKeyDown(KeyCode.G) && highlightIsEnabled)
        {
            turnOffHighlights();            
        }
    }

    private void turnOffOutlines()
    {
        outlineIsEnabled = false;
        foreach (GameObject outline in outlines)
        {
            outline.SetActive(false);
        }        
    }
    private void turnOffHighlights()
    {
        highlightIsEnabled = false;
        foreach (GameObject highlight in highlights)
        {
            highlight.SetActive(false);
        }
    }
}
