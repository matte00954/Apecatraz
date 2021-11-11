//Author: William �rnquist
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    //'public' necessery to reference a specific state in other scripts (if they have a reference to EnemyMovement).
    public enum GuardState { patrolling, waiting, investigating, dumbstruck, chasing, shooting } 
    private GuardState currentState;

    //Default spacing from agent destinations
    private const float DEFAULT_MIN_SPACING = 1f;
    //Default timer reset value
    private const float TIMER_RESET_VALUE = 0f;
    private const float DEFAULT_PATROL_SPEED = 3f;
    private const float DEFAULT_ALERT_SPEED = 6f;

    [Header("References")]
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip alertClip;
    [SerializeField] private AudioClip dumbstruckClip;
    [SerializeField] private AudioClip lostplayerClip;
    [SerializeField] private AudioClip mistakenClip;
    private AudioSource audioSource;

    [Header("Agent")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField, Range(1f, 10f)] private float patrolSpeed = DEFAULT_PATROL_SPEED;
    [SerializeField, Range(1f, 10f)] private float alertSpeed = DEFAULT_ALERT_SPEED;

    [SerializeField, Range(1f, 10f)]
    private float patrolDestinationSpacing = DEFAULT_MIN_SPACING;
    [SerializeField, Range(1f, 10f)]
    private float investigateDestinationSpacing = DEFAULT_MIN_SPACING;
    [SerializeField, Range(1f, 10f)]
    private float chaseDestinationSpacing = DEFAULT_MIN_SPACING;

    [Header("Detection")]
    [SerializeField] private GameObject playerDetectionPoint;
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask detectionMask;
    [SerializeField] private Collider fieldOfViewCollider;
    [SerializeField] private Transform headTransform;
    [SerializeField, Tooltip("(Debug setting) If set true, the enemy will be unable to detect the player through line of sight.")]
    private bool isBlind;
    [SerializeField, Tooltip("(Debug setting) If set true, the enemy will be unable to detect luring sounds.")]
    private bool isDeaf;
    [SerializeField, Range(0f, 5f), Tooltip("The amount of time before the player's position becomes unknown to the enemy. (chase-state only)")]
    private float lostDetectionDelay;
    [SerializeField, Range(0f, 5f), Tooltip("The amount of time the enemy stays dumbstruck in place before reacting.")]
    private float dumbstruckTime;

    [Header("Patrol variables")]
    [SerializeField, Tooltip("A stationary guard will stay and move back to one spot.")]
    private bool isStationary;
    [SerializeField, Tooltip("Check this if this guard follows an encircling path.")]
    private bool isCircling;
    [SerializeField, Tooltip("The amount of time the guard should wait after reaching a waypoint.")]
    private float totalWaitTime;
    
    [SerializeField, Tooltip("An array of waypoint objects the guard should follow in order.")]
    private GameObject[] waypoints;

    private Vector3 lastKnownPlayerDestination;

    private int currentWaypointIndex;
    private float waitStateTimer;
    private float dumbStateTimer;
    private float detectionTimer;
    private float detectionRange;

    private bool isPathInverted;
    private bool detectingPlayer;

    public void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        detectionRange = fieldOfViewCollider.bounds.size.z;
        agent.speed = patrolSpeed;
        audioSource.clip = walkClip;
        audioSource.loop = true;

        if (isStationary)
            Debug.Log(gameObject.name + " is set to stationary.");
        else if (agent == null)
            Debug.LogError("The nav mesh agent component is not attached to " + gameObject.name + ".");
        else if (waypoints == null || waypoints.Length <= 1)
        {
            Debug.LogError("Insufficient waypoints for basic patrolling behaviour. Setting 'isStationary' to TRUE");
            isStationary = true;
        }
        else
        {
            currentWaypointIndex = 0;
            currentState = GuardState.patrolling;
        }
    }

    void Update()
    {
        UpdateDetectionRays();
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case GuardState.patrolling:
                if (agent.remainingDistance <= patrolDestinationSpacing)
                {
                    audioSource.Pause();
                    NextWaypoint();
                    StartWaiting();
                }
                break;
            case GuardState.waiting:
                if (waitStateTimer < totalWaitTime)
                    waitStateTimer += Time.deltaTime;
                else
                {
                    waitStateTimer = 0f;
                    currentState = GuardState.patrolling;

                    ResumePatrol();
                }
                break;
            case GuardState.investigating:
                if (agent.remainingDistance <= investigateDestinationSpacing)
                    StartWaiting();
                break;
            case GuardState.dumbstruck: //While dumbstruck is active, the enemy will stand still and rotate towards the player until the dumbstruck timer runs out.
                if (dumbStateTimer < dumbstruckTime) 
                {
                    dumbStateTimer += Time.deltaTime;
                    RotateSelfToPlayer();
                }
                else if (detectingPlayer) //This runs once when enemy sees player at the end of dumbstruck time which transitions from 'dumbstruck' to 'chasing'.
                {
                    exclamationMark.SetActive(true);
                    audioSource.Stop();
                    audioSource.PlayOneShot(alertClip);
                    dumbStateTimer = TIMER_RESET_VALUE;
                    currentState = GuardState.chasing;
                    agent.speed = alertSpeed;
                }
                else //This runs once when enemy does not see the player at the end of dumbstruck time which transitions from 'dumbstruck' to 'waiting'.
                {
                    audioSource.PlayOneShot(mistakenClip);
                    dumbStateTimer = TIMER_RESET_VALUE;
                    StartWaiting();
                }
                break;
            case GuardState.chasing:
                if (detectionTimer < lostDetectionDelay)
                {
                    agent.SetDestination(playerDetectionPoint.transform.position);
                    detectionTimer += Time.deltaTime;
                }

                if (detectingPlayer)
                    detectionTimer = TIMER_RESET_VALUE;
                else if (agent.remainingDistance <= chaseDestinationSpacing && detectionTimer >= lostDetectionDelay)
                {
                    exclamationMark.SetActive(false);
                    audioSource.PlayOneShot(lostplayerClip);
                    StartWaiting();
                }
                break;
            case GuardState.shooting:
                if (agent.remainingDistance >= 1f)
                    agent.SetDestination(transform.position);
                break;
            default:
                Debug.LogError("currentState is NULL!");
                break;
        }
    }

    private void StartWaiting()
    {
        currentState = GuardState.waiting;
        agent.SetDestination(transform.position);
        waitStateTimer = TIMER_RESET_VALUE;
        agent.speed = patrolSpeed;
    }

    private void UpdateDetectionRays()
    {
        detectingPlayer = false;
        if (!isBlind && Physics.Raycast(headTransform.position, (playerDetectionPoint.transform.position - headTransform.position).normalized,
            out RaycastHit sightHit, detectionRange, detectionMask))
            if (sightHit.collider.gameObject.CompareTag("Player") && fieldOfViewCollider.bounds.Contains(playerDetectionPoint.transform.position))
            {
                detectingPlayer = true;
                ReactToPlayer();
            }
    }

    private void ReactToPlayer()
    {
        if (currentState.Equals(GuardState.patrolling) || currentState.Equals(GuardState.waiting) || currentState.Equals(GuardState.investigating))
        {
            currentState = GuardState.dumbstruck;
            audioSource.Stop();
            audioSource.PlayOneShot(dumbstruckClip);
            agent.SetDestination(transform.position);
        }
        else if (currentState != GuardState.dumbstruck && currentState != GuardState.shooting)
        {
            currentState = GuardState.chasing;
            detectionTimer = TIMER_RESET_VALUE;
        }
    }

    private void NextWaypoint()
    {
        if (!isPathInverted)
        {
            currentWaypointIndex++;

            if (isCircling && currentWaypointIndex >= waypoints.Length)
                currentWaypointIndex = 0;
            else if (!isCircling && currentWaypointIndex >= waypoints.Length)
            {
                //Since the order is now in reverse, we decrement to 1 index lower than the last index as our next waypoint.
                currentWaypointIndex = waypoints.Length - 2;
                isPathInverted = true;
            }
        }
        else
        {
            currentWaypointIndex--;

            if (currentWaypointIndex < 0)
            {
                //Since index 0 is already reached at this point, next index will be 1. (resetting to 0 will make the guard wait twice as long)
                currentWaypointIndex = 1;
                isPathInverted = false;
            }
        }
    }

    private void ResumePatrol()
    {
        audioSource.Play();
        agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
    }

    public void RotateSelfToPlayer()
    {
        Vector3 direction = (PlayerDetectionPosition - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
    }

    public void RefreshDetectionDelay()
    {
        detectionTimer = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentState == GuardState.patrolling || currentState == GuardState.waiting)
        {
            if (!isDeaf && other.gameObject.CompareTag("LuringSource"))
            {
                currentState = GuardState.investigating;
                agent.SetDestination(other.gameObject.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerDetectionPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(headTransform.position, (playerDetectionPoint.transform.position - headTransform.position).normalized * detectionRange);
        }
    }

    //Gets & sets
    public Vector3 HeadPosition { get => headTransform.position; }
    public Vector3 PlayerDetectionPosition { get => playerDetectionPoint.transform.position; }
    public NavMeshAgent Agent { get => agent; }
    public bool DetectingPlayer { get => detectingPlayer; }
    public GuardState CurrentState { get => currentState; set => currentState = value; }
    
}