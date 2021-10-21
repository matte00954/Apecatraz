using System.Collections.Generic;
using UnityEngine;

public class ClimbTransforms : MonoBehaviour
{
    public List<Transform> climbList = new List<Transform>();

    public Transform GetClimbPositionInList(int index)
    {
        return climbList[index];
    }
}
