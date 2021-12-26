// Author: Andreas Scherman
using UnityEngine;

public class GatherColliders : MonoBehaviour
{
    [SerializeField] private GameObject[] gameObjects;

    private void Start()
    {
        gameObjects = GameObject.FindGameObjectsWithTag("3DUI");

        DrawLines();
    }

    ////private void Update()
    ////{
    ////    ////OnDrawGizmos();
    ////}

    private void DrawLines()
    {
        foreach (GameObject game in gameObjects)
        {
            GameObject uiWall = Instantiate(game, transform);
            uiWall.layer = 15;
            uiWall.AddComponent<LineRenderer>();

            LineRenderer line = uiWall.GetComponent<LineRenderer>();
            ////Collider collider = uiWall.GetComponent<Collider>(); // Unused
            Mesh mesh = uiWall.GetComponent<MeshFilter>().mesh;

            uiWall.GetComponent<MeshRenderer>().enabled = false;

            line.SetWidth(0.1f, 0.1f); // TODO: Solve obsolete issue
            line.useWorldSpace = false;

            line.positionCount = mesh.vertices.Length;
            line.SetPositions(mesh.vertices);
        }
    }
}
