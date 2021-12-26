// Author: Andreas Scherman
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuManager : MonoBehaviour
{
    private static bool gameIsPaused = false;

    private Animator animator;

    [SerializeField] private GameObject[] menuItems;
    [SerializeField] private DialogueManager dialogueManager;

    public static bool GameIsPaused { get => gameIsPaused; set => gameIsPaused = value; }

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
        foreach (GameObject menu in menuItems)
            menu.SetActive(false);

        menuItems[menuNumber].SetActive(true);
    }

    ////public void OpenOptions() { }
    public void OpenMainMenu() => SceneManager.LoadScene("MainMenu");
    public void OpenConfirmMenu(int menuNumber) => menuItems[menuNumber].SetActive(true);
    public void CloseConfirmMenu(int menuNumber) => menuItems[menuNumber].SetActive(false);

    private void Awake()
    {
        Time.timeScale = 1;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!gameIsPaused)
                    PauseGame();
                else
                    ResumeGame();
            }

            if (Input.GetKeyDown(KeyCode.Q))
                dialogueManager.DisplayNextSentence();

            if (Input.GetKeyDown(KeyCode.Z))
                dialogueManager.EndDialogue();
        }
    }
}
