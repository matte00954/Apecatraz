using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyChain : MonoBehaviour
{
    private List<string> Keycards = new List<string>();

    public void AddKeyCard(string Color)
    {
        Keycards.Add(Color);
    }

    public void RemoveKeyCard(string Color)
    {
        Keycards.Remove(Color);
    }

    public void ResetKeyCards()
    {
        Keycards.Clear();
    }

    public bool CheckIfKeycardIsInKeychain(string Color)
    {
        return Keycards.Contains(Color);
    }
}
