using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharAnims : MonoBehaviour
{
    [SerializeField]
    private GameObject ledgeDistanceRay;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private ThirdPersonMovement tpm;

    private GameObject player;
    private float saveRotation;

    // Start is called before the first frame update
    void Start()
    {
        player = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        GetTurn();
        if (tpm.PlayerState.Equals(ThirdPersonMovement.State.nothing))
        {
            RaycastHit hit;

            if (Physics.Raycast(ledgeDistanceRay.transform.position, ledgeDistanceRay.transform.forward,
                out hit, 1f)) //checks distance from object so animation starts at correct the distance
            {
                anim.SetFloat("ledge", Vector3.Distance(ledgeDistanceRay.transform.position, hit.point));

            }
            else
            {
                anim.SetFloat("ledge", 0);
            }
        }
    }

    public void CheckStopRunning() //Joche
    {
        if (tpm.GetVelocity() > 3)
        {
            /*if (tpm.PlayerState.Equals(ThirdPersonMovement.State.climbing))
            {
                anim.SetTrigger("Start");
            }*/
            if (!tpm.GetIsMoving())
            {
                anim.SetTrigger("Start");
            }
            tpm.SetIsMoving(true);
        }
        if (tpm.GetVelocity() < 3 && tpm.GetIsMoving())
        {
            tpm.SetIsMoving(false);
            anim.SetTrigger("Stop");
        }
    }


    public void SetTriggerFromString(string trigger)
    {
        anim.SetTrigger(trigger);
    }

    public void SetAnimFloat(string animName, float magnitude)
    {
        anim.SetFloat(animName, magnitude);
    }

    public void SetAnimBool(string animName, bool value)
    {
        anim.SetBool(animName, value);
    }

    private void GetTurn() //Joche
    {
        anim.SetFloat("runX", (saveRotation + 1000) - (transform.eulerAngles.y + 1000));
        saveRotation = transform.eulerAngles.y;
    }
}
