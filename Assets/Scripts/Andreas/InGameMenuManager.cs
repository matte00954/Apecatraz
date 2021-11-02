using UnityEngine;

public class InGameMenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;
    
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameIsPaused)
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
        animator.SetTrigger("Close");
        
        Time.timeScale = 1; 
        gameIsPaused = false;

    }

    public void PauseGame()
    {
        animator.SetTrigger("Open");
        
        Time.timeScale = 0;
        gameIsPaused = true;
    }

}
