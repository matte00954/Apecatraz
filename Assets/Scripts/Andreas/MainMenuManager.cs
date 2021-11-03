using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{   
    public void LoadPrototype(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
