//Author: William Örnquist
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private const float MIN_SPACING_PATROL = 1f;
    private const float MIN_SPACING_INVESTIGATE = 10f;
    private const float MIN_SPACING_CHASE = 5f;

    [Header("Agent")]
    [SerializeField] private NavMeshAgent agent;

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
    [SerializeField, Range(1f, 10f)]
    private float stopDistancePatrol = 1f;
    [SerializeField, Tooltip("An array of waypoint objects the guard should follow in order.")]
    private GameObject[] waypoints;

    private int currentWaypointIndex;

    private float waitStateTimer;
    private float dumbStateTimer;
    private float detectionTimer;
    private float detectionRange;

    private bool isPathInverted;
    private bool detectedPlayerOnce;
    private bool detectingPlayer;

    private enum GuardState { patrolling, waiting, investigating, dumbstruck, chasing }
    private GuardState currentState;

    public void Start()
    {
        detectionRange = fieldOfViewCollider.bounds.size.z;
        detectedPlayerOnce = false;

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

    //public bool TryGetComponent<T>(this GameObject obj, T result) where T : Component
    //{
    //    return (result = obj.GetComponent<T>()) != null;
    //}

    private void UpdateState()
    {
        switch (currentState)
        {
            case GuardState.patrolling:
                if (agent.remainingDistance <= MIN_SPACING_PATROL)
                    StartWaiting();
                break;
            case GuardState.waiting:
                if (waitStateTimer < totalWaitTime)
                    waitStateTimer += Time.deltaTime;
                else
                {
                    waitStateTimer = 0f;
                    currentState = GuardState.patrolling;

                    NextWaypoint();
                    ResumePatrol();
                }
                break;
            case GuardState.investigating:
                if (agent.remainingDistance <= MIN_SPACING_INVESTIGATE)
                    StartWaiting();
                break;
            case GuardState.dumbstruck:
                if (dumbStateTimer < dumbstruckTime)
                    dumbStateTimer += Time.deltaTime;
                else if (detectingPlayer)
                {
                    //Play "alert" voice here
                    currentState = GuardState.chasing;
                }
                else
                {
                    //Play "mistaken" voice here
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
                    detectionTimer = 0f;
                else if (agent.remainingDistance <= MIN_SPACING_CHASE && detectionTimer >= lostDetectionDelay)
                    StartWaiting();
                break;
            default:
                Debug.LogError("currentState is NULL!");
                break;
        }
    }

    private void StartWaiting()
    {
        currentState = GuardState.waiting;
        waitStateTimer = 0f;
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
        if (!detectedPlayerOnce)
        {
            currentState = GuardState.dumbstruck;
            agent.SetDestination(transform.position);
            detectedPlayerOnce = true;
        }
        else if (currentState != GuardState.dumbstruck)
        {
            currentState = GuardState.chasing;
            detectionTimer = 0f;
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
        agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
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
    public Transform HeadTransform { get => headTransform; }
    public GameObject PlayerDetectionPoint { get => playerDetectionPoint; }
}