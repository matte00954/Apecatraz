using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    [SerializeField] private Transform holdParent;

    [SerializeField] private float moveForce = 250f;

    [SerializeField] private float pickupRange = 6f;

    [SerializeField] ThirdPersonMovement thirdPersonMovement;

    private GameObject carriedObject;

    [SerializeField] private LayerMask canBeCarriedLayer;

    private bool carrying;

    void Start()
    {
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
            MoveObject();
        }
    }

    private void FindObject()
    {

        if (carriedObject == null)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
                Debug.Log("hit " + hit.transform.gameObject);
                thirdPersonMovement.ActivateRenderer(1); // 1 = Ability shader
                thirdPersonMovement.PlayerState = ThirdPersonMovement.State.telekinesis;
            }
        }
        else
            DropObject();
    }

    private void PickupObject(GameObject pickObject)
    {
        if (pickObject.GetComponent<Rigidbody>())
        {
            Rigidbody objectRigidbody = pickObject.GetComponent<Rigidbody>();
            objectRigidbody.useGravity = false;
            objectRigidbody.drag = 2f; //Makes object move slower when holding
            //objectRigidbody.transform.parent = holdParent;
            carriedObject = pickObject;
        }
    }

    private void MoveObject()
    {
        if(Vector3.Distance(carriedObject.transform.position, holdParent.position) > 0.1f)
        {
            Vector3 moveDirection = holdParent.position - carriedObject.transform.position;
            carriedObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);

            //RaycastHit hit;

            /*if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                DropObject();
            }*/
        }
    }

    private void DropObject()
    {
        thirdPersonMovement.ActivateRenderer(0); // 0 = default shader
        Rigidbody carriedRigidbody = carriedObject.GetComponent<Rigidbody>();
        carriedRigidbody.useGravity = true;
        carriedRigidbody.drag = 1f;
        carriedObject.transform.parent = null;
        carriedObject = null;
        thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position , transform.TransformDirection(Vector3.forward) * pickupRange);
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