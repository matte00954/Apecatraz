using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private void Awake()
    {
        Time.timeScale = 1;
    }
    public void LoadPrototype(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
