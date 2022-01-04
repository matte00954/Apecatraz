// Author: Andreas Scherman
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class InGameMenuManager : MonoBehaviour
{
    private static bool gameIsPaused = false;

    private Animator animator;

    [SerializeField] private GameObject[] menuItems;
    [SerializeField] private DialogueManager dialogueManager;


    [Header("First Selected item")]
    [SerializeField] private GameObject inGameMenuFirstButton;
    [SerializeField] private GameObject optionsMenuFirstButton;
    [SerializeField] private GameObject gameMenuFirstButton;
    [SerializeField] private GameObject audioMenuFirstButton;
    [SerializeField] private GameObject videoMenuFirstButton;
    [SerializeField] private GameObject accessabilityMenuFirstButton;
    [SerializeField] private GameObject colorMenuFirstButton;
    [SerializeField] private GameObject fontMenuFirstButton;
    [SerializeField] private GameObject areYouSureMenuFirstButton;

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

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void PauseGame()
    {
        animator.SetTrigger("Open");

        OpenMenu(0);
        Time.timeScale = 0;
        gameIsPaused = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inGameMenuFirstButton);

    }

    public void OpenMenu(int menuNumber)
    {
        foreach (GameObject menu in menuItems)
            menu.SetActive(false);

        menuItems[menuNumber].SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        if (menuNumber == 0) EventSystem.current.SetSelectedGameObject(inGameMenuFirstButton);
        if (menuNumber == 1) EventSystem.current.SetSelectedGameObject(optionsMenuFirstButton);
        if (menuNumber == 2) EventSystem.current.SetSelectedGameObject(gameMenuFirstButton);
        if (menuNumber == 3) EventSystem.current.SetSelectedGameObject(audioMenuFirstButton);
        if (menuNumber == 4) EventSystem.current.SetSelectedGameObject(videoMenuFirstButton);
        if (menuNumber == 5) EventSystem.current.SetSelectedGameObject(accessabilityMenuFirstButton);
        if (menuNumber == 6) EventSystem.current.SetSelectedGameObject(colorMenuFirstButton);
        if (menuNumber == 7) EventSystem.current.SetSelectedGameObject(fontMenuFirstButton);
        if (menuNumber == 8) EventSystem.current.SetSelectedGameObject(areYouSureMenuFirstButton);
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
        //if (Input.anyKeyDown)
        //{
        if (Input.GetButtonDown("escape"))
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
        //}
    }
}
