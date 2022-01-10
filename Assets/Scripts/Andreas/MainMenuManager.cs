// Author: Andreas Scherman
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;

    [Header("First Selected item")] // jacobs kod, behövs för min xbox kontroller
    [SerializeField] private GameObject startFirstButton;


    public void LoadPrototype(string sceneName) => SceneManager.LoadScene(sceneName);

    public void ActivateImage(GameObject image) //Author: Hannas Hellberg
    {
        mainPanel.SetActive(false);
        image.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);// jacobs kod, behövs för min xbox kontroller
        EventSystem.current.SetSelectedGameObject(image.transform.GetChild(0).gameObject);// jacobs kod, behövs för min xbox kontroller
    }

    public void DisableImage(GameObject image) //Author: Hanna Hellberg
    {
        image.SetActive(false);
        mainPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);// jacobs kod, behövs för min xbox kontroller
        EventSystem.current.SetSelectedGameObject(startFirstButton);// jacobs kod, behövs för min xbox kontroller
    }

    public void QuitGame() => Application.Quit();

    private void Awake() => Time.timeScale = 1;
}
