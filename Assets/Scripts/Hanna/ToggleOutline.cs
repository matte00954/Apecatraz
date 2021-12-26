// Author: Hanna Hellberg
using UnityEngine;

public class ToggleOutline : MonoBehaviour
{
    public GameObject[] outlines;
    public GameObject[] highlights;

    private bool outlineIsEnabled;
    private bool highlightIsEnabled;

    private void Start()
    {
        TurnOffOutlines();
        TurnOffHighlights();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !outlineIsEnabled)
        {
            outlineIsEnabled = true;            
            foreach (GameObject outline in outlines)
            {
                outline.SetActive(true);                
            }     
        }
        else if (Input.GetKeyDown(KeyCode.T) && outlineIsEnabled)
        {            
            TurnOffOutlines();            
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
            TurnOffHighlights();            
        }
    }

    private void TurnOffOutlines()
    {
        outlineIsEnabled = false;
        foreach (GameObject outline in outlines)
        {
            outline.SetActive(false);
        }        
    }

    private void TurnOffHighlights()
    {
        highlightIsEnabled = false;
        foreach (GameObject highlight in highlights)
        {
            highlight.SetActive(false);
        }
    }
}
