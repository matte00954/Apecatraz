// Author: Mattias Larsson
// Secondary Author: William �rnquist
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Windows.Speech;
using Cinemachine;

public class Telekinesis : MonoBehaviour
{
    // Readonly fields (make serializable?)
    private readonly float minDrag = 1f;
    private readonly float maxDrag = 4f;
    private readonly float moveForce = 50f;
    private readonly float pickupRange = 8f;
    private readonly float minRange = 1.5f;
    private readonly float maxRange = 12f; // Needs to be higher than pickuprange
    private readonly float telekinesisSphereRadius = 3f;
    private readonly float pushMultiplier = 20f;
    private readonly float maxPushTimer = 1f;
    private readonly float telekinesisEnergyCost = 0.1f;

    [Header("Game Object references")]
    [SerializeField] private ThirdPersonMovement thirdPersonMovement;
    [SerializeField] private Transform cameraTelekinesisTarget;
    [SerializeField] private CinemachineFreeLook cinemachine;
    [SerializeField] private LayerMask canBeCarriedLayer;
    [SerializeField] private LayerMask canBePushedLayer;

    [Header("Energy")]
    [SerializeField] private Energy energy;

    [Header("Telekinesis")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject telekinesisOrigin;
    private bool voiceCommandsEnabled = true;

    [SerializeField] private AudioClip telekinesisSound;
    [SerializeField] private AudioClip pushSound;
    [SerializeField] private AudioSource audioSource;

    private int telkenesisOffset = 0;
    private int maxTelekenesisOffset = 3;
    private int minTelekenesisOffset = -1;

    // Added by Emil
    private Material originalMat;
    [SerializeField] private Material telekinesisMat;

    private GameObject carriedObject;

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

    public bool VoiceCommandsEnabled { get => voiceCommandsEnabled; set => voiceCommandsEnabled = value; }

    ////private List<GameObject> collisionContacts = new List<GameObject>();

    private void Start()
    {
        /*foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }*/

        if (audioSource == null)
        {
            Debug.LogError("No audio source");
        }

        if (voiceCommandsEnabled)
        {
            /// Voice commands
            keywordActions.Add("Forward", VoiceForwardOne);
            keywordActions.Add("Go", VoiceForwardOne);

            keywordActions.Add("Back", VoiceBackOne);
            keywordActions.Add("Come", VoiceBackOne);

            keywordActions.Add("Full forward", VoiceFullForward);
            keywordActions.Add("Forward max", VoiceFullForward);
            keywordActions.Add("All forward", VoiceFullForward);
            keywordActions.Add("Full go", VoiceFullForward);

            keywordActions.Add("Full back", VoiceFullBack);
            keywordActions.Add("All back", VoiceFullBack);
            keywordActions.Add("Come back", VoiceFullBack);
            keywordActions.Add("Get back", VoiceFullBack);


            keywordActions.Add("Pick", VoicePickUp);
            keywordActions.Add("Pick up", VoicePickUp);

            keywordActions.Add("Drop", VoiceDrop);

            keywordActions.Add("Fly", VoiceUp);

            keywordActions.Add("Up", VoiceUp);

            keywordActions.Add("Upp", VoiceUp);

            keywordActions.Add("Op", VoiceUp);

            keywordActions.Add("Opp", VoiceUp);

            keywordActions.Add("Down", VoiceDown);

            keywordActions.Add("Do", VoiceDown);

            keywordActions.Add("Dow", VoiceDown);

            keywordActions.Add("Le", VoiceLeft);

            keywordActions.Add("Lef", VoiceLeft);

            keywordActions.Add("Left", VoiceLeft);

            keywordActions.Add("Ri", VoiceRight);

            keywordActions.Add("Rig", VoiceRight);

            keywordActions.Add("Righ", VoiceRight);

            keywordActions.Add("Right", VoiceRight);

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
        FindObject(true);
    }

    private void VoiceDrop()
    {
        DropObject();
    }
    private void VoiceLeft()
    {
        VoiceMoveXValue(-5);
    }

    private void VoiceRight()
    {
        VoiceMoveXValue(5);
    }

    private void VoiceUp()
    {
        VoiceMoveYValue(-0.1f);
    }

    private void VoiceDown()
    {
        VoiceMoveYValue(0.1f);
    }


    // TODO
    // Audio feedback everything voice command related
    // Up,down,left,right
    #endregion
    private void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            FindObject(false);
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

    private void VoiceMoveXValue(int i)
    {
        cinemachine.m_XAxis.Value = i;
    }

    private void VoiceMoveYValue(float f)
    {
        cinemachine.m_YAxis.Value = f;
    }

    private void FindObject(bool voiceCommand)
    {
        if (carriedObject == null && !silenced && energy.CheckEnergy(telekinesisEnergyCost))
        {
            // Multiple raycasts do not seem to work in the same if statement, therefore split up
            if (Physics.Raycast(telekinesisOrigin.transform.position, mainCamera.transform.TransformDirection(Vector3.forward), out RaycastHit hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
            }
            else if (Physics.SphereCast(telekinesisOrigin.transform.position, telekinesisSphereRadius, mainCamera.transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBeCarriedLayer))
            {
                PickupObject(hit.transform.gameObject);
            }
            else if (Physics.SphereCast(telekinesisOrigin.transform.position, telekinesisSphereRadius, mainCamera.transform.TransformDirection(Vector3.forward), out hit, pickupRange, canBePushedLayer))
            { // Sphere moving towards camera forward
                PushObject(hit);
            }
        }
        else
        {
            if (voiceCommand == false)
                DropObject();
        }
    }

    private void PushObject(RaycastHit hit) // Push instead of moving object
    {
        audioSource.clip = pushSound;
        audioSource.Play();

        if (pushTimer <= 0f)
        {
            Rigidbody push = hit.transform.gameObject.CompareTag("WreckingBall") ?
                hit.transform.gameObject.GetComponentInParent<Rigidbody>()
                : hit.transform.gameObject.GetComponent<Rigidbody>();
            
            ////if (hit.transform.gameObject.CompareTag("WreckingBall"))
            ////{
            ////    push = hit.transform.gameObject.GetComponentInParent<Rigidbody>();
            ////}
            ////else
            ////{
            ////    push = hit.transform.gameObject.GetComponent<Rigidbody>();
            ////}
            
            if (push == null)
                return;

            push.AddForce(mainCamera.transform.TransformDirection(Vector3.forward) * pushMultiplier, ForceMode.Impulse);
            pushTimer = maxPushTimer;
        }
    }

    private void PickupObject(GameObject pickObject)
    {
        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = telekinesisSound;
        audioSource.Play();

        if (pickObject.GetComponent<Rigidbody>())
        {
            thirdPersonMovement.ActivateRenderer(1); //// 1 = Ability shader
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.telekinesis;

            Rigidbody objectRigidbody = pickObject.GetComponent<Rigidbody>();
            objectRigidbody.useGravity = false;
            objectRigidbody.freezeRotation = true;
            objectRigidbody.drag = maxDrag; // Makes object move slower when holding
            carriedObject = pickObject;

            // Emil's Shader
            originalMat = carriedObject.GetComponent<Renderer>().material;
            telekinesisMat.SetTexture("MainTexture", originalMat.mainTexture);
            carriedObject.GetComponent<Renderer>().material = telekinesisMat;

            // Destroy Icon
            Destroy(carriedObjectOutline);

            // Activate vfx
            thinking.Play();
            telekinesis.enabled = true;
        }
    }

    private void MoveObject()
    {
        energy.IsRegenerating = false;
        if (Vector3.Distance(carriedObject.transform.position, cameraTelekinesisTarget.position) > 0.1f)
        {
            Vector3 moveDirection = cameraTelekinesisTarget.position - carriedObject.transform.position + (cameraTelekinesisTarget.forward * telkenesisOffset);
            carriedObject.GetComponent<Rigidbody>().AddForce(moveDirection * moveForce);
            
            // Set telekinesis target
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
            audioSource.loop = false;
            audioSource.Stop();

            thirdPersonMovement.ActivateRenderer(0); // 0 = default shader
            Rigidbody carriedRigidbody = carriedObject.GetComponent<Rigidbody>();
            carriedRigidbody.freezeRotation = false;
            carriedRigidbody.useGravity = true;
            carriedRigidbody.drag = minDrag;

            // Added by Emil
            carriedObject.GetComponent<Renderer>().material = originalMat;
            originalMat = null;

            carriedObject.transform.parent = null;
            carriedObject = null;
            thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;

            telkenesisOffset = 0;
            energy.IsRegenerating = true;
            
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