// Author: [full name here]
using UnityEngine;

public class CharAnims : MonoBehaviour
{
    private const float LedgeRayDistance = 1f;

    [SerializeField]
    private GameObject ledgeDistanceRay;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private ThirdPersonMovement tpm;

    private GameObject player;
    private float saveRotation;

    public void SetTriggerFromString(string trigger) => anim.SetTrigger(trigger);
    public void SetAnimFloat(string animName, float magnitude) => anim.SetFloat(animName, magnitude);
    public void SetAnimBool(string animName, bool value) => anim.SetBool(animName, value);
    public void CheckStopRunning() // Added by Joche
    {
        if (tpm.Velocity > 3)
        {
            /*if (tpm.PlayerState.Equals(ThirdPersonMovement.State.climbing))
            {
                anim.SetTrigger("Start");
            }*/
            if (!tpm.IsMoving)
                anim.SetTrigger("Start");

            tpm.IsMoving = true;
        }

        if (tpm.Velocity < 3 && tpm.IsMoving)
        {
            tpm.IsMoving = false;
            anim.SetTrigger("Stop");
        }
    }

    private void Start() => player = this.gameObject;

    private void Update()
    {
        GetTurn();
        if (tpm.PlayerState.Equals(ThirdPersonMovement.State.nothing))
        {
            // Checks distance from object so animation starts at correct the distance
            if (Physics.Raycast(ledgeDistanceRay.transform.position, ledgeDistanceRay.transform.forward, out RaycastHit hit, LedgeRayDistance))
                anim.SetFloat("ledge", Vector3.Distance(ledgeDistanceRay.transform.position, hit.point));
            else
                anim.SetFloat("ledge", 0);
        }
    }

    private void GetTurn() // Added by Joche
    {
        anim.SetFloat("runX", (saveRotation + 1000) - (transform.eulerAngles.y + 1000));
        saveRotation = transform.eulerAngles.y;
    }
}
