using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnims : MonoBehaviour
{

    [SerializeField]
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMove(int move)
    {
        anim.SetFloat("Blend", move);
    }

    public void Aim()
    {
        anim.SetTrigger("Aim");
    }

    public void StopAiming()
    {
        anim.SetTrigger("StopAiming");
    }

    public void Fire()
    {
        anim.SetTrigger("Fire");
    }
}
