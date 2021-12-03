using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashcanTransform : MonoBehaviour
{
    [SerializeField]
    private Transform enterLocation;

    public Transform GetEnterLocation()
    {
        return enterLocation;
    }
}
