using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;

    [SerializeField] private GameObject player;

    private CharacterController characterController;

    private GameObject carriedObject;

    private LayerMask canBeCarried;

    private bool carrying;

    private float distance;

    private float smooth;

    void Start()
    {
        characterController = player.GetComponent<CharacterController>();
        carriedObject = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            FindObject();
        }

        if (carriedObject != null)
        {
            Carry();
        }

        /*if (PlayerState.current == PlayerState.State.carrying)
        {

        }*/
    }

    private void Carry()
    {
        carriedObject.GetComponent<Rigidbody>().AddForce(new Vector3(0, 2f, 0));
    }

    private void FindObject()
    {
        RaycastHit hit;

        Debug.DrawRay(player.transform.position, player.transform.forward * 5f, Color.red, 10f);

        if (Physics.Raycast(player.transform.position, player.transform.forward * 5f, out hit, 10f, canBeCarried))
        {
            Debug.Log("asasdasd");
            carriedObject = hit.transform.gameObject;
            Debug.Log(carriedObject);
        }
        else
            return;
    }

    private void Drop()
    {

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