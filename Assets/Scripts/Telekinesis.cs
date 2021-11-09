using UnityEngine;

public class Telekinesis : MonoBehaviour
{
    [Header("Game Object references")]
    [SerializeField] ThirdPersonMovement thirdPersonMovement;
    [SerializeField] private Transform cameraTelekinesisTarget;
    [SerializeField] private LayerMask canBeCarriedLayer;

    [Header("Energy")]
    [SerializeField] private Energy energy;
    private float telekinesisEnergyCost = 0.1f;

    [Header("Telekinesis")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject telekinesisOrigin;

    private float telkenesisOffsetMultiplier;

    private GameObject carriedObject;

    private float moveForce = 10f;

    private float pickupRange = 7f;

    private float minRange = 1.5f;
    private float maxRange = 12.5f; //needs to be higher than pickuprange

    private bool silenced;

    void Start()
    {
        carriedObject = null;
        telkenesisOffsetMultiplier = 0f;
    }

    private void Update()
    {
        if (thirdPersonMovement.isTelekinesisActive)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                FindObject();
            }

            if (carriedObject != null)
            {
                MoveObject();
            }
            else
                telkenesisOffsetMultiplier = 0f;
        }
    }

    private void FindObject()
    {

        if (carriedObject == null && !silenced && energy.CheckEnergy(telekinesisEnergyCost))
        {
            RaycastHit hit;

            if (Physics.Raycast(telekinesisOrigin.transform.position, /*transform.TransformDirection(Vector3.forward)*/ mainCamera.transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
                //Debug.Log("hit " + hit.transform.gameObject);
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
            objectRigidbody.freezeRotation = true;
            objectRigidbody.drag = 6f; //Makes object move slower when holding
            carriedObject = pickObject;
        }
    }

    private void MoveObject()
    {
        if(Vector3.Distance(carriedObject.transform.position, cameraTelekinesisTarget.position) > 0.1f)
        {
            Vector3 moveDirection = cameraTelekinesisTarget.position - carriedObject.transform.position + (cameraTelekinesisTarget.forward * telkenesisOffsetMultiplier);
            carriedObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);

            energy.SpendEnergy(telekinesisEnergyCost);

            if(!energy.CheckEnergy(telekinesisEnergyCost))
            {
                DropObject();
                return;
            }

            if (Vector3.Distance(transform.position, carriedObject.transform.position) > maxRange)
            {
                DropObject();
                return;
            }

            if(Vector3.Distance(transform.position, carriedObject.transform.position) < minRange)
            {
                DropObject();
                return;
            }

            if (Input.mouseScrollDelta.y > 0)
                telkenesisOffsetMultiplier += Input.mouseScrollDelta.y; 
            if (Input.mouseScrollDelta.y < 0)
                telkenesisOffsetMultiplier -= -Input.mouseScrollDelta.y; //do not touch
        }
    }

    private void DropObject()
    {
        if(carriedObject != null)
        {
            thirdPersonMovement.ActivateRenderer(0); // 0 = default shader
            Rigidbody carriedRigidbody = carriedObject.GetComponent<Rigidbody>();
            carriedRigidbody.freezeRotation = false;
            carriedRigidbody.useGravity = true;
            carriedRigidbody.drag = 1f;
            carriedObject.transform.parent = null;
            carriedObject = null;
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;
        }
        else
            Debug.LogError("carriedObject is null");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("AntiAbilityZone"))
        {
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;
            DropObject();
            silenced = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("AntiAbilityZone"))
        {
            silenced = false;
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.forward));
        //For testing
    }
}