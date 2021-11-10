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

    private GameObject carriedObject;

    private float moveForce = 5f;

    private float pickupRange = 6f;

    private float maxRange = 15f; //needs to be higher than pickuprange

    private bool silenced;

    public VisualEffect thinking;
    
    //Render for the carriedObject
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineScaleFactor;
    [SerializeField] private Color outlineColor;
    private Renderer outlineRenderer;

    private GameObject carriedObjectOutline;

    public Canvas interactiveIcon;


    void Start()
    {
        carriedObject = null;
     
        //Vfx
        thinking.Stop();

        //
        carriedObjectOutline = null;
        //OutlineRenderer
        //outlineRenderer = CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor);

    }

    private void Update()
    {
        FindObjectDemo();

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

        if (carriedObject == null && !silenced && energy.CheckEnergy(telekinesisEnergyCost))
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
                Debug.Log("hit " + hit.transform.gameObject);
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
            objectRigidbody.drag = 2f; //Makes object move slower when holding
            carriedObject = pickObject;

            //Outline the object that would be hit curently
            //CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor, carriedObject);

            //Destroy Icon
            Debug.Log(carriedObjectOutline);
            Destroy(carriedObjectOutline);
        }
    }

    private void MoveObject()
    {
        if(Vector3.Distance(carriedObject.transform.position, cameraTelekinesisTarget.position) > 0.1f)
        {
            Vector3 moveDirection = cameraTelekinesisTarget.position - carriedObject.transform.position;
            carriedObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);

            energy.SpendEnergy(telekinesisEnergyCost);

            if(!energy.CheckEnergy(telekinesisEnergyCost))
            {
                DropObject();
            }

            if (Vector3.Distance(transform.position, carriedObject.transform.position) > maxRange)
            {
                DropObject();
            }
        }
    }

    private void DropObject()
    {
        if(carriedObject != null)
        {
            thirdPersonMovement.ActivateRenderer(0); // 0 = default shader
            Rigidbody carriedRigidbody = carriedObject.GetComponent<Rigidbody>();
            carriedRigidbody.useGravity = true;
            carriedRigidbody.drag = 1f;
            carriedObject.transform.parent = null;
            carriedObject = null;
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;
            
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
   
    //Add a outline to the object
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
    private void FindObjectDemo()
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