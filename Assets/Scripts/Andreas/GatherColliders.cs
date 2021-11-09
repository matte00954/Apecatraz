using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GatherColliders : MonoBehaviour
{
    public GameObject[] gObjects;
    // Start is called before the first frame update
    void Start()
    {
        gObjects = GameObject.FindGameObjectsWithTag("3DUI");

        DrawLines();
    }

    // Update is called once per frame
    void Update()
    {
        //OnDrawGizmos();
    }


    private void DrawLines()
    {
        foreach (GameObject game in gObjects)
        {
            game.AddComponent<LineRenderer>();

            LineRenderer line = game.GetComponent<LineRenderer>();
            Collider collider = game.GetComponent<Collider>();
            Mesh mesh = game.GetComponent<MeshFilter>().mesh;

            line.SetWidth(0.01f, 0.01f);
            line.useWorldSpace =false;

            line.positionCount = mesh.vertices.Length;
            line.SetPositions(mesh.vertices);
        }
    }
    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (Mesh box in boxes)
        {
            Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
        }
    }
    */
}
