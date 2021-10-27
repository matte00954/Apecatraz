using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telekinesis : MonoBehaviour
{

    [SerializeField] private GameObject mainCamera;

    private GameObject player;

    private CharacterController characterController;

    private GameObject carriedObject;

    private bool carrying;

    private float distance;

    private float smooth;

    void Start()
    {
        //mainCamera = GameObject.FindWithTag("MainCamera");
        player = this.gameObject;
        characterController = player.GetComponent<CharacterController>();
    }


    /*

    void Update()
    {
        if (carrying)
        {
            Carry(carriedObject);
            CheckDrop();
        }
        else
        {
            Pickup();
        }
    }


    void Carry(GameObject o)
    {
        RaycastHit hit;
        Physics.SphereCast(o.transform.position, o.GetComponent<BoxCollider>().bounds.size.y / 2, transform.forward, out hit);
        Debug.Log(hit);
        o.transform.position = Vector3.Lerp(o.transform.position, player.transform.position + player.transform.forward * distance, Time.deltaTime * smooth);
        o.transform.rotation = Quaternion.identity;
    }

    void Pickup()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 startPositionForRay = player.transform.position + characterController.center;
            RaycastHit hit;
            if (Physics.SphereCast(startPositionForRay, characterController.height / 2, player.transform.forward, out hit, distance))
            {
                GameObject p = hit.collider.gameObject;
                Debug.Log(p);
                if (p.CompareTag("moveable"))
                {
                    carrying = true;
                    carriedObject = p;
                    carriedObject.GetComponent<Rigidbody>().useGravity = false;
                }
            }
        }
    }

    void CheckDrop()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            dropObject();
        }
    }

    void dropObject()
    {
        carrying = false;
        carriedObject.gameObject.GetComponent<Rigidbody>().useGravity = true;
        carriedObject = null;
    }*/
}