using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMarkerTest : MonoBehaviour
{
    public LineRenderer line;
    public Transform boxPos;
    public Transform playerPos;

    // Start is called before the first frame update
    void Start()
    {
        line.positionCount = 2;
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, boxPos.position);
        line.SetPosition(1, playerPos.position);
    }
}
