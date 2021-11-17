using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeAnims : MonoBehaviour
{
    public GameObject raycast1;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim= GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(raycast1.transform.position, raycast1.transform.forward * 1,
            out hit, .6f)) //checks distance from object so animation starts at correct the distance
        {
            anim.SetFloat("ledge", Vector3.Distance(raycast1.transform.position, hit.point)/ 0.6f);

        }
        else
        {
            anim.SetFloat("ledge", 0);
        }
    }
}
