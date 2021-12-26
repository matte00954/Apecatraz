// Author: [full name here]
using UnityEngine.SceneManagement;

public static class ResetScene
{
    public static void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
