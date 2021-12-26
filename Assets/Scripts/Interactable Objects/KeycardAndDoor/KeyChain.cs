using System.Collections.Generic;
using UnityEngine;

public class KeyChain : MonoBehaviour
{
    private List<string> keycards = new List<string>();

    public void AddKeyCard(string color) => keycards.Add(color);
    public void RemoveKeyCard(string color) => keycards.Remove(color);
    public void ResetKeyCards() => keycards.Clear(); 
    public bool IsKeycardInKeychain(string color) { return keycards.Contains(color); }
}
