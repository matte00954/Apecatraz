// Author: Emil Moqvist
using UnityEngine;

public class LineMarkerTest : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform boxPos;
    [SerializeField] private Transform playerPos;

    private void Start() => line.positionCount = 2;

    private void Update()
    {
        line.SetPosition(0, boxPos.position);
        line.SetPosition(1, playerPos.position);
    }
}
