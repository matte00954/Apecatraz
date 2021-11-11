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
            GameObject UIWall = Instantiate(game, transform);
            UIWall.layer = 15;
            UIWall.AddComponent<LineRenderer>();

            LineRenderer line = UIWall.GetComponent<LineRenderer>();
            Collider collider = UIWall.GetComponent<Collider>();
            Mesh mesh = UIWall.GetComponent<MeshFilter>().mesh;

            UIWall.GetComponent<MeshRenderer>().enabled = false;

            line.SetWidth(0.1f, 0.1f);
            line.useWorldSpace =false;

            line.positionCount = mesh.vertices.Length;
            line.SetPositions(mesh.vertices);
        }
    }
}
