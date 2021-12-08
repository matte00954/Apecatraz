// Author: Mattias Larsson
// Author: William �rnquist
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Windows.Speech;

public class Telekinesis : MonoBehaviour
{
    [Header("Game Object references")]
    [SerializeField] private ThirdPersonMovement thirdPersonMovement;
    [SerializeField] private Transform cameraTelekinesisTarget;
    [SerializeField] private LayerMask canBeCarriedLayer;
    [SerializeField] private LayerMask canBePushedLayer;


    [Header("Energy")]
    [SerializeField] private Energy energy;
    private float telekinesisEnergyCost = 0.1f;

    [Header("Telekinesis")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject telekinesisOrigin;
    [SerializeField] private bool voiceCommandsEnabled = false;

    private int telkenesisOffset = 0;
    private int maxTelekenesisOffset = 3;
    private int minTelekenesisOffset = -1;

    //Emil
    private Material originalMat;
    [SerializeField] private Material telekinesisMat;

    private float minDrag = 1f;
    private float maxDrag = 4f;

    private GameObject carriedObject;

    private float moveForce = 50f;
    private float pickupRange = 8f;
    private float minRange = 1.5f;
    private float maxRange = 12f; // needs to be higher than pickuprange
    private float telekinesisSphereRadius = 3f;

    private float pushMultiplier = 20f;
    private float maxPushTimer = 1f;
    private float pushTimer;

    private bool silenced;

    /// Added by Andreas 2021-11-11
    //// Render for the carriedObject
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineScaleFactor;
    [SerializeField] private Color outlineColor;

    private Renderer outlineRenderer;
    private GameObject carriedObjectOutline;

    [SerializeField] private Canvas interactiveIcon;
    [SerializeField] private VisualEffect thinking;
    [SerializeField] private VisualEffect telekinesis;

    // Telekinesis prototype
    private Dictionary<string, Action> keywordActions = new Dictionary<string, Action>();
    private KeywordRecognizer keywordRecognizer;

    ////private List<GameObject> collisionContacts = new List<GameObject>();

    private void Start()
    {
        /*foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }*/

        if (voiceCommandsEnabled)
        {
            /// Voice commands
            keywordActions.Add("Forward", VoiceForwardOne);
            keywordActions.Add("Back", VoiceBackOne);

            keywordActions.Add("Full forward", VoiceFullForward);
            keywordActions.Add("Full back", VoiceFullBack);

            keywordActions.Add("Pick", VoicePickUp);
            keywordActions.Add("Pick up", VoicePickUp);

            keywordActions.Add("Drop", VoiceDrop);

            keywordRecognizer = new KeywordRecognizer(keywordActions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += OnKeyWordsRecognized;
            keywordRecognizer.Start();
        }

        /// Inking and typing settings, get to know you needs to be on
        
        // vfx
        thinking.Stop();
        telekinesis.enabled = false;
        carriedObjectOutline = null;

        carriedObject = null;
        telkenesisOffset = 0;
    }

    #region Mattias Telekinesis prototype

    private void OnKeyWordsRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log("Keyword :" + args.text);
        keywordActions[args.text].Invoke();
    }

    private void VoiceFullForward()
    {
        IncreaseTelekinesisOffset(Mathf.Abs(minTelekenesisOffset - maxTelekenesisOffset));
    }

    private void VoiceFullBack()
    {
        DecreaseTelekinesisOffset(Mathf.Abs(minTelekenesisOffset - maxTelekenesisOffset));
    }

    private void VoiceForwardOne()
    {
        IncreaseTelekinesisOffset(1);
    }

    private void VoiceBackOne()
    {
        DecreaseTelekinesisOffset(1);
    }

    private void VoicePickUp()
    {
        FindObject();
    }

    private void VoiceDrop()
    {
        DropObject();
    }

    // TODO
    // Audio feedback everything voice command related
    // Up,down,left,right
    #endregion
    private void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            FindObject();
        }

        if (pushTimer > 0f)
        {
            pushTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        // Enable for outline of objects.
        // FindObjectOutline();
        if (thirdPersonMovement.IsTelekinesisActive)
        {
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
            //Multiple raycasts do not seem to work in the same if statement, therefore split up
            if (Physics.Raycast(telekinesisOrigin.transform.position,
                mainCamera.transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
            }
            else if (Physics.SphereCast(telekinesisOrigin.transform.position, telekinesisSphereRadius,
                    mainCamera.transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
            }
            else if (Physics.SphereCast(telekinesisOrigin.transform.position, telekinesisSphereRadius,
                    mainCamera.transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBePushedLayer)) //Sphere moving towards camera forward
            {
                PushObject(hit);
            }
        }
        else
            DropObject();
    }

    private void PushObject(RaycastHit hit) //Push instead of moving object
    {
        if (pushTimer <= 0f)
        {
            Rigidbody push = null;
            if (hit.transform.gameObject.CompareTag("WreckingBall"))
            {
                push = hit.transform.gameObject.GetComponentInParent<Rigidbody>();
            }
            else
            {
                push = hit.transform.gameObject.GetComponent<Rigidbody>();
            }
            if (push == null)
                return;

            push.AddForce(mainCamera.transform.TransformDirection(Vector3.forward) * pushMultiplier, ForceMode.Impulse);
            pushTimer = maxPushTimer;
        }
    }

    private void PickupObject(GameObject pickObject)
    {
        if (pickObject.GetComponent<Rigidbody>())
        {
            thirdPersonMovement.ActivateRenderer(1); //// 1 = Ability shader
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.telekinesis;

            Rigidbody objectRigidbody = pickObject.GetComponent<Rigidbody>();
            objectRigidbody.useGravity = false;
            objectRigidbody.freezeRotation = true;
            objectRigidbody.drag = maxDrag; // Makes object move slower when holding
            carriedObject = pickObject;

            //Emil Shader
            originalMat = carriedObject.GetComponent<Renderer>().material;
            telekinesisMat.SetTexture("MainTexture", originalMat.mainTexture);
            carriedObject.GetComponent<Renderer>().material = telekinesisMat;

            // Destroy Icon
            Destroy(carriedObjectOutline);

            //activate vfx
            thinking.Play();
            telekinesis.enabled = true;

        }
    }

    private void MoveObject()
    {
        energy.ActivateEnergyRegen(false);
        if (Vector3.Distance(carriedObject.transform.position, cameraTelekinesisTarget.position) > 0.1f)
        {
            Vector3 moveDirection = cameraTelekinesisTarget.position - carriedObject.transform.position + (cameraTelekinesisTarget.forward * telkenesisOffset);
            carriedObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);
            
            //Set telekinesis target,
            telekinesis.SetVector3("TargetVector3", carriedObject.GetComponent<Rigidbody>().worldCenterOfMass);

            energy.SpendEnergy(telekinesisEnergyCost); 

            if (!energy.CheckEnergy(telekinesisEnergyCost)
                || Vector3.Distance(transform.position, carriedObject.transform.position) > maxRange          
                /*|| collisionContacts.Contains(carriedObject)*/)
            {
                DropObject();
                return;
            }

            if (Input.mouseScrollDelta.y > 0)
            {
                IncreaseTelekinesisOffset((int)Input.mouseScrollDelta.y);
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                DecreaseTelekinesisOffset(-(int)Input.mouseScrollDelta.y);
            }
            else if (Input.GetKey(KeyCode.Alpha1))
            {
                IncreaseTelekinesisOffset(1);
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                DecreaseTelekinesisOffset(1);
            }
        }
    }

    private void IncreaseTelekinesisOffset(int amount)
    {
        if (telkenesisOffset <= maxTelekenesisOffset)
            telkenesisOffset += amount;
    }

    private void DecreaseTelekinesisOffset(int amount)
    {
        if (telkenesisOffset >= minTelekenesisOffset)
            telkenesisOffset -= amount;
    }

    private void DropObject()
    {
        if (carriedObject != null)
        {
            thirdPersonMovement.ActivateRenderer(0); // 0 = default shader
            Rigidbody carriedRigidbody = carriedObject.GetComponent<Rigidbody>();
            carriedRigidbody.freezeRotation = false;
            carriedRigidbody.useGravity = true;
            carriedRigidbody.drag = minDrag;
            //Emil
            carriedObject.GetComponent<Renderer>().material = originalMat;
            originalMat = null;

            carriedObject.transform.parent = null;
            carriedObject = null;
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;

            telkenesisOffset = 0;
            energy.ActivateEnergyRegen(true);
            
            // Stops vfx and objectOutline
            thinking.Stop();
            telekinesis.enabled = false;
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

    private void OnCollisionStay(Collision collision)
    {
        if (carriedObject != null)
        {
            if (collision.gameObject.Equals(carriedObject))
            {
                DropObject();
            }
        }
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Contact added to list");
        collisionContacts.Add(collision.gameObject);
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Contact removed from list");
        collisionContacts.Remove(collision.gameObject);
    }
    */

    #region Andreas Outline code
    // NOT USED

    /// This is how a outline is created
    private Renderer CreateOutline(Material outlineMat, float scaleFactor, Color color, GameObject hit)
    {
        GameObject outlineObject = Instantiate(hit.gameObject, hit.transform.position, hit.transform.rotation, hit.transform);

        carriedObjectOutline = outlineObject;
        Debug.Log(carriedObjectOutline);
        //// Renderer previousRend;

        Canvas newIcon = Instantiate(interactiveIcon, hit.transform.position + Vector3.up, hit.transform.rotation.normalized, carriedObjectOutline.transform);

        Renderer rend = outlineObject.GetComponent<Renderer>();

        rend.material = outlineMat;
        rend.material.SetColor("_OutlineColor", color);
        rend.material.SetFloat("_Scale", scaleFactor);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Destroy(outlineObject.GetComponent<Rigidbody>());
        Destroy(outlineObject.GetComponent<Collider>());
        //// rend.enabled = false;

        return rend;
    }

    private void FindObjectOutline()
    {
        if (carriedObjectOutline == null && !silenced && energy.CheckEnergy(telekinesisEnergyCost))
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                // Outline the object that would be hit curently
                if (carriedObject == null)
                {
                    CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor, hit.transform.gameObject);
                }
            }
        }
    }
    #endregion
}