using UnityEngine;

public class SecurityCam : MonoBehaviour
{
    // Read-only variables
    private const string PlayerTagStr = "Player";
    private const float TimerResetValue = 0f;

    // Serialized variables
    [Header("References")]
    [SerializeField] private GameObject objectRotator;
    [SerializeField] private Collider revealerCollider;
    [SerializeField] private Light lightDetection;
    [SerializeField] private Color colorSafe;
    [SerializeField] private Color colorDetecting;
    [SerializeField] private Color colorAlert;
    [SerializeField] private AudioClip clipRotation;
    [SerializeField] private AudioClip clipDetecting;
    [SerializeField] private AudioClip clipAlert;
    private AudioSource audioSource;
    private Collider scannerCollider;

    [Header("Rotation attributes")]
    [SerializeField, Range(1f, 20f), Tooltip("The amount of time it takes before the camera starts rotating to a different direction.")]
    private float scanTimeInSeconds = 1f;
    [SerializeField, Range(1f, 90f), Tooltip("How fast the camera rotates.")]
    private float rotationSpeed = 1f;
    [SerializeField, Range(0f, 90f), Tooltip("How many degrees the camera can rotate to the 'left' from its centered rotation.")]
    private float leftScanAngle;
    [SerializeField, Range(0f, 90f), Tooltip("How many degrees the camera can rotate to the 'right' from its centered rotation.")]
    private float rightScanAngle;

    [Header("Detection attributes")]
    [SerializeField, Range(1f, 20f), Tooltip("The amount of time the player has to be within view before the camera becomes alerted.")]
    private float detectionBuffer;
    [SerializeField, Range(1f, 20f), Tooltip("The amount of time camera stays alerted before reverting to 'safe'.")]
    private float alertEndDelay;
    [SerializeField, Range(1f, 50f), Tooltip("The maximum raycast distance for detecting player.")]
    private float detectionRayDistance;
    [SerializeField, Tooltip("Which layers the raycast collides with.")]
    private LayerMask rayLayerMask;

    // Non-serialized variables
    private Vector3 leftScanEuler, rightScanEuler;

    private float scanTimer;
    private float detectionTimer;
    private float alertDelayTimer;

    private bool rotatingClockwise;
    private bool playerIsDetectable;
    private bool isAlerted;

    public void ForceEndAlert()
    {
        if (isAlerted)
        {
            isAlerted = false;
            alertDelayTimer = TimerResetValue;
        }
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        scannerCollider = GetComponent<Collider>();
        leftScanEuler = objectRotator.transform.rotation.eulerAngles - (Vector3.up * leftScanAngle);
        rightScanEuler = objectRotator.transform.rotation.eulerAngles + (Vector3.up * rightScanAngle);
    }

    private void Start() => GameManager.SecurityCams.Add(gameObject);

    private void Update()
    {
        if (Time.timeScale > 0f)
        {
            UpdateScanTime();

            if (rotatingClockwise)
                Rotate(rightScanEuler);
            else
                Rotate(leftScanEuler);

            if (isAlerted)
                UpdateAlert();
            else if (playerIsDetectable)
                UpdateDetection();
            else if (detectionTimer > 0f)
            {
                detectionTimer = TimerResetValue;
                lightDetection.color = colorSafe;
            }
            else if (audioSource.clip != clipRotation)
                audioSource.clip = clipRotation;
            else if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (Time.timeScale == 0f && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void UpdateScanTime()
    {
        if (scanTimer >= scanTimeInSeconds)
        {
            scanTimer = TimerResetValue;
            rotatingClockwise = !rotatingClockwise;
        }
        else
            scanTimer += Time.deltaTime;
    }

    private void UpdateAlert()
    {
        if (alertDelayTimer >= alertEndDelay)
        {
            lightDetection.color = colorSafe;
            alertDelayTimer = TimerResetValue;
            isAlerted = false;
            return;
        }
        else
            alertDelayTimer += Time.deltaTime;

        if (playerIsDetectable)
        {
            alertDelayTimer = TimerResetValue;
            if (!revealerCollider.enabled)
                revealerCollider.enabled = true;
        }
        else if (revealerCollider.enabled)
            revealerCollider.enabled = false;

        if (audioSource.clip != clipAlert)
            audioSource.clip = clipAlert;
        else if (!audioSource.isPlaying)
            audioSource.Play();
    }

    private void UpdateDetection()
    {
        if (detectionTimer == 0f && !isAlerted)
            lightDetection.color = colorDetecting;

        if (detectionTimer >= detectionBuffer)
            StartAlert();
        else
            detectionTimer += Time.deltaTime;

        if (audioSource.clip != clipDetecting)
            audioSource.clip = clipDetecting;
        else if (!audioSource.isPlaying)
            audioSource.Play();
    }

    private void StartAlert()
    {
        isAlerted = true;
        lightDetection.color = colorAlert;
    }

    private void Rotate(Vector3 scanEuler)
    {
        float angle = Mathf.MoveTowardsAngle(objectRotator.transform.eulerAngles.y, scanEuler.y, rotationSpeed * Time.deltaTime);
        objectRotator.transform.eulerAngles = new Vector3(0, angle, 0);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(PlayerTagStr))
            playerIsDetectable = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(PlayerTagStr))
        {
                if (Physics.Raycast(transform.position, (other.gameObject.transform.position - transform.position).normalized, out RaycastHit sightHit, detectionRayDistance, rayLayerMask))
                    if (sightHit.collider.CompareTag(PlayerTagStr) && scannerCollider.bounds.Contains(other.gameObject.transform.position))
                        playerIsDetectable = true;
                    else
                        playerIsDetectable = false;
        }
    }
}