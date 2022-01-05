// Author: [full name here]
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnims : MonoBehaviour
{
    private const string BlendStr = "Blend";
    private const string AimStr = "Aim";
    private const string StopAimingStr = "StopAiming";
    private const string FireStr = "Fire";
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private NavMeshAgent agent;

    private float lastFrameVelocity= 0;

    public void SetMove(int move) => anim.SetFloat(BlendStr, move);
    public void Aim() => anim.SetTrigger(AimStr);
    public void StopAiming() => anim.SetTrigger(StopAimingStr);
    public void Fire() => anim.SetTrigger(FireStr);
    public void TriggerFromString(string name) => anim.SetTrigger(name);
    public void BoolFromString(string name, bool state) => anim.SetBool(name, state);

    public bool Accelerating()
    {
        if (lastFrameVelocity< agent.velocity.magnitude)
        {
            if (agent.velocity.magnitude - lastFrameVelocity> 0.1f)
            {
                lastFrameVelocity = agent.velocity.magnitude;
                return true;
            }
        }
        lastFrameVelocity = agent.velocity.magnitude;
        return false;
    }
}
