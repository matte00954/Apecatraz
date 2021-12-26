// Author: Andreas Scherman
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{    
    [SerializeField] private GameObject mainPanel;

    public void LoadPrototype(string sceneName) => SceneManager.LoadScene(sceneName);

    public void ActivateImage(GameObject image)
    {
        mainPanel.SetActive(false);
        image.SetActive(true);
    }

    public void DisableImage(GameObject image)
    {
        image.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void QuitGame() => Application.Quit();

    private void Awake() => Time.timeScale = 1;
}
