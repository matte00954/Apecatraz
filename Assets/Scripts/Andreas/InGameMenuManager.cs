using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    public static bool gameIsPaused = false;
    
    private Animator animator;

    [SerializeField] GameObject[] MenuItems;
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
        SceneManager.LoadScene(0);

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

        OpenMenu(0);
        Time.timeScale = 0;
        gameIsPaused = true;
    }

    public void OpenMenu(int menuNumber)
    {
        foreach (GameObject menu in MenuItems)
        {
            menu.SetActive(false);
        }
        MenuItems[menuNumber].SetActive(true);
    }

    public void OpenOptions()
    {

    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenConfirmMenu(int menuNumber)
    {
        MenuItems[menuNumber].SetActive(true);
    }
    public void CloseConfirmMenu(int menuNumber)
    {
        MenuItems[menuNumber].SetActive(false);
    }

}
