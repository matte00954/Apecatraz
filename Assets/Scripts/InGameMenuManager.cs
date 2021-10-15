using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TEMPESC();
    }

    public void QuitGame()
    {

    }
    public void ExitToMainMenu()
    {
        Debug.Log("Application has been terminated");
        Application.Quit();
    }

    public void TEMPESC()
    {
        if (Input.GetKey("escape"))
        {
            animator.SetTrigger("OpenInGameMenu");
        }
    }

    public void ResumeGame()
    {

    }

    public void PauseGame()
    {

    }
}
