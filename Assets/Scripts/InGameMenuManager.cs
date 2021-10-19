using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!GameIsPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void QuitGame()
    {
        Debug.Log("Application has been terminated");
        Application.Quit();
    }
    public void ExitToMainMenu()
    {
        Debug.Log("Switched to Main Menu");

    }

    public void ResumeGame()
    {
        animator.SetTrigger("CloseInGameMenu");

        //Time.timeScale = 1;
        GameIsPaused = false;

    }

    public void PauseGame()
    {
        animator.SetTrigger("OpenInGameMenu");

        //Time.timeScale = 0;
        GameIsPaused = true;
    }
}
