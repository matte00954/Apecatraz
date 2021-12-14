using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BananaPickup : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0, 90 * Time.deltaTime, 0);
    }

}
