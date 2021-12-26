// Author: Andreas Scherman
using UnityEngine;
using UnityEngine.VFX;

public class MonkeyVFX : MonoBehaviour
{
    private CharacterController controller;
    private ThirdPersonMovement thirdPersonMovement;

    [SerializeField] private VisualEffect dustTrail;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        thirdPersonMovement = GetComponent<ThirdPersonMovement>();

        ////dustTrail.Stop();
    }

    private void Update()
    {
        {
           dustTrail.Play(); // ???
        }
    }
}
