// Author: [full name here]
using UnityEngine;

public class EnemyAnims : MonoBehaviour
{
    private const string BlendStr = "Blend";
    private const string AimStr = "Aim";
    private const string StopAimingStr = "StopAiming";
    private const string FireStr = "Aim";
    [SerializeField]
    private Animator anim;

    public void SetMove(int move) => anim.SetFloat(BlendStr, move);
    public void Aim() => anim.SetTrigger(AimStr);
    public void StopAiming() => anim.SetTrigger(StopAimingStr);
    public void Fire() => anim.SetTrigger(FireStr);
    public void TriggerFromString(string name) => anim.SetTrigger(name);
    public void BoolFromString(string name, bool state) => anim.SetBool(name, state);
}
