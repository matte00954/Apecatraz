// Author: [full name here]
using UnityEngine;

public class TrashcanTransform : MonoBehaviour
{
    [SerializeField] private Transform enterLocation;
    [SerializeField] private string objectName;
    public Transform GetEnterLocation()
    {
        objectName = gameObject.name;
        Debug.Log("used trashcan number :" + objectName);
        return enterLocation;
    }
}
