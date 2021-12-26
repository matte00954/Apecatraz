// Author: [full name here]
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeLoader : MonoBehaviour
{
    public static void StartPrototype(string prototypeName)
    {
        SceneManager.LoadScene("prototypeName");
    }
}
