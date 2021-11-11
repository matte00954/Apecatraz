//Author: Mattias Larsson
//Author: William Örnquist
using UnityEngine;
using UnityEngine.VFX;

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

    private float telkenesisOffsetMultiplier = 0f;

    private GameObject carriedObject;

    private float moveForce = 10f;

    private float pickupRange = 7f;

    private float minRange = 1.5f;
    private float maxRange = 12.5f; //needs to be higher than pickuprange

    private bool silenced;

    /// Added by Andreas 2021-11-11
    //Render for the carriedObject
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineScaleFactor;
    [SerializeField] private Color outlineColor;

    private Renderer outlineRenderer;
    private GameObject carriedObjectOutline;

    public Canvas interactiveIcon;    
    public VisualEffect thinking;
    ///

    void Start()
    {
        carriedObject = null;
        telkenesisOffsetMultiplier = 0f;

        ///
        thinking.Stop();
        carriedObjectOutline = null;
        ///
    }

    private void Update()
    {
        //Enable for outline of objects.
        //FindObjectOutline();

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

                //Makes the VFX play
                thinking.Play();
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

            //Destroy Icon
            Destroy(carriedObjectOutline);
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
            if (Input.mouseScrollDelta.y < 0 && telkenesisOffsetMultiplier > 0)
            {
                telkenesisOffsetMultiplier -= -Input.mouseScrollDelta.y;
                Debug.Log(telkenesisOffsetMultiplier);
            } 
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

            telkenesisOffsetMultiplier = 0f;

            //Stops vfx and objectOutline
            thinking.Stop();
            Destroy(carriedObjectOutline);
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

    /// This is how a outline is created
    Renderer CreateOutline(Material outlineMat, float scaleFactor, Color color, GameObject hit)
    {
        GameObject outlineObject = Instantiate(hit.gameObject, hit.transform.position, hit.transform.rotation, hit.transform);

        carriedObjectOutline = outlineObject;
        Debug.Log(carriedObjectOutline);
        //Renderer previousRend;

        Canvas newIcon = Instantiate(interactiveIcon, hit.transform.position + (Vector3.up), (hit.transform.rotation.normalized), carriedObjectOutline.transform);

        Renderer rend = outlineObject.GetComponent<Renderer>();

        rend.material = outlineMat;
        rend.material.SetColor("_OutlineColor", color);
        rend.material.SetFloat("_Scale", scaleFactor);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Destroy(outlineObject.GetComponent<Rigidbody>());
        Destroy(outlineObject.GetComponent<Collider>());
        //rend.enabled = false;

        return rend;
    }
    private void FindObjectOutline()
    {
        if (carriedObjectOutline == null && !silenced && energy.CheckEnergy(telekinesisEnergyCost))
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                //Outline the object that would be hit curently
                if (carriedObject == null)
                {
                    CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor, hit.transform.gameObject);
                }
            }
        }
    }
}