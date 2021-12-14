using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WonkyBanana : MonoBehaviour
{

   /* void Start()
    {
        pos = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1, this.gameObject.transform.position.z);

    } */
    void Update()
    {
        transform.Rotate(90 * Time.deltaTime * 10, 0, 0);

    }
}
