using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashcanTransform : MonoBehaviour
{
    [SerializeField]
    private Transform enterLocation;
    [SerializeField] private string name;
    public Transform GetEnterLocation()
    {
        name = gameObject.name;
        Debug.Log("used trashcan number :" + name);
        return enterLocation;
    }
}
