using UnityEngine.SceneManagement;
using UnityEngine;

public class PrototypeLoader : MonoBehaviour
{
    public static void StartPrototype(string prototypeName)
    {
        SceneManager.LoadScene("prototypeName");
    }
}
