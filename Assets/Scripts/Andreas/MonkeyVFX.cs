using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class MonkeyVFX : MonoBehaviour
{
    CharacterController controller;
    ThirdPersonMovement thirdPersonMovement;

    public VisualEffect dustTrail;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        thirdPersonMovement = GetComponent<ThirdPersonMovement>();

        //dustTrail.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (!thirdPersonMovement.Moving)
        {
           dustTrail.Play();
        }
    }
}
