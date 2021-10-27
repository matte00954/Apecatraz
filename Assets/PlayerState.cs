using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public enum State { blinking, inAir, carrying }
    public static State current { get; set; }
}
