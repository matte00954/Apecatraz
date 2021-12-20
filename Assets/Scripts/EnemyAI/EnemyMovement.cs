// Author: William �rnquist
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Default spacing from agent destinations
    private const float DefaultMinSpacing = 1f;

    // Default timer reset value
    private const float TimerResetValue = 0f;
    private const float DefaultPatrolSpeed = 3f;
    private const float DefaultAlertSpeed = 6f;

    private const float StationaryLookAtPositionMagnitude = 5f;

    // This list keeps an eye on all enemies that are currently aware of the player's position
    private static List<GameObject> awareEnemies = new List<GameObject>();

    [Header("References")]
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip alertClip;
    [SerializeField] private AudioClip dumbstruckClip;
    [SerializeField] private AudioClip lostplayerClip;
    [SerializeField] private AudioClip mistakenClip;
    private AudioSource audioSource;
    [SerializeField] private AudioClip[] chasingClips;
    private EnemyAnims enemyAnim;
    [SerializeField] private EnemyVariables enemyVariables;
    private string path;
    private TextAsset jsonFile;


    [Header("Agent")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField, Range(1f, 10f)] private float patrolSpeed = DefaultPatrolSpeed;
    [SerializeField, Range(1f, 10f)] private float alertSpeed = DefaultAlertSpeed;

    [SerializeField, Range(1f, 10f)]
    private float patrolDestinationSpacing = DefaultMinSpacing;
    [SerializeField, Range(1f, 10f)]
    private float investigateDestinationSpacing = DefaultMinSpacing;
    [SerializeField, Range(1f, 10f)]
    private float chaseDestinationSpacing = DefaultMinSpacing;

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
    [SerializeField, Range(0f, 10f), Tooltip("The amount of time the enemy spends randomly searching around the last known location of the player.")]
    private float searchTime;
    [SerializeField, Range(1f, 30f), Tooltip("The maximum distance from the next search point.")]
    private float searchPointRadius;

    [Header("Patrol variables")]
    [SerializeField, Tooltip("A stationary guard will stay and move back to one spot.")]
    private bool isStationary;
    [SerializeField, Range(0f, 180f), Tooltip("Changes how many degrees the enemy will look around if stationary.")]
    private float scanRotation;
    [SerializeField, Range(1f, 10f), Tooltip("The frequency of looking left and right if stationary.")]
    private float scanTime;
    [SerializeField, Range(1f, 100f), Tooltip("How fast the enemy rotates if stationary.")]
    private float scanSpeed;
    [SerializeField, Tooltip("Check this if this guard follows an encircling path.")]
    private bool isCircling;
    [SerializeField, Tooltip("The amount of time the guard should wait after reaching a waypoint.")]
    private float totalWaitTime;
    [SerializeField, Tooltip("An array of waypoint objects the guard should follow in order.")]
    private GameObject[] waypoints;

    private Quaternion startingRotation;
    private Vector3 startingPosition;
    private Vector3 stationaryPosition;
    private Vector3 stationaryLookAtPosition;
    private Vector3 leftScanAngle;
    private Vector3 rightScanAngle;

    private int currentWaypointIndex;
    private float waitStateTimer;
    private float dumbStateTimer;
    private float searchTimer;
    private float detectionTimer;
    private float detectionRange;
    private float stationaryScanTimer;

    private bool isPathInverted;
    private bool detectingPlayer;
    private bool searchPointSet;
    private bool rightScanIsNext;
    private GuardState currentState;

    // 'public' necessery to reference a specific state in other scripts (if they have a reference to EnemyMovement).
    public enum GuardState { patrolling, waiting, scanning, investigating, searching, dumbstruck, chasing, shooting }

    // Gets & sets

    /// <summary>
    /// Keeps an eye on all enemies that are currently aware of the player's current position.
    /// </summary>
    public static List<GameObject> AwareEnemies { get => awareEnemies; }
    public Vector3 HeadPosition { get => headTransform.position; }
    public Vector3 PlayerDetectionPosition { get => playerDetectionPoint.transform.position; }
    public NavMeshAgent Agent { get => agent; }
    public bool IsDetectingPlayer { get => detectingPlayer; }
    public GuardState CurrentState { get => currentState; set => currentState = value; }

    public string guardNumber = "";

    /// <summary>
    /// Rotates the enemy towards the specified position.
    /// </summary>
    /// <param name="pos">Position</param>
    public void RotateSelfToPosition(Vector3 pos, float ratio)
    {
        Vector3 direction = (pos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * ratio);
    }

    /// <summary>
    /// Rotates the enemy towards the player's position.
    /// </summary>
    public void RotateSelfToPlayer() => RotateSelfToPosition(PlayerDetectionPosition, 3f);

    public void RefreshDetectionDelay() => detectionTimer = 0f;

    /// <summary>
    /// Enemy starts waiting in place for X seconds set from totalWaitTime.
    /// </summary>
    public void StartWaiting()
    {
        if (currentState.Equals(GuardState.chasing))
            enemyAnim.StopAiming();

        currentState = GuardState.waiting;
        agent.SetDestination(transform.position);
        waitStateTimer = TimerResetValue;
        agent.speed = patrolSpeed;
    }

    public void ResetTransform()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }

    private void Start()
    {
        path = Application.streamingAssetsPath + "/EnemyVariables.json";
        string contents = File.ReadAllText(path);
        EnemyVariables EnemyMovementInJson = JsonUtility.FromJson<EnemyVariables>(contents);
        patrolSpeed = EnemyMovementInJson.patrolSpeed;
        alertSpeed = EnemyMovementInJson.alertSpeed;
        dumbstruckTime = EnemyMovementInJson.dumbstruckTime;
    }

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        enemyAnim = GetComponentInChildren<EnemyAnims>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        detectionRange = fieldOfViewCollider.bounds.size.z;
        agent.speed = patrolSpeed;
        audioSource.clip = walkClip;
        audioSource.loop = true;

        if (waypoints.Length <= 1 && !isStationary)
        {
            Debug.LogWarning("Insufficient waypoints for basic patrolling behaviour. Setting " + gameObject.name + " 'isStationary' to TRUE");
            isStationary = true;
        }

        if (isStationary)
        {
            stationaryPosition = transform.position;
            stationaryLookAtPosition = stationaryPosition + (transform.forward * StationaryLookAtPositionMagnitude);
            leftScanAngle = transform.rotation.eulerAngles - (Vector3.up * scanRotation);
            rightScanAngle = transform.rotation.eulerAngles + (Vector3.up * scanRotation);
            currentState = GuardState.waiting;
        }
        else if (waypoints.Length > 1)
        {
            currentWaypointIndex = 0;
            currentState = GuardState.patrolling;
        }
    }

    private void Update()
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
                else if (!isStationary)
                {
                    waitStateTimer = 0f;
                    currentState = GuardState.patrolling;

                    ResumePatrol();
                }
                else if ((transform.position - stationaryPosition).magnitude >= 1f)
                    agent.SetDestination(stationaryPosition);
                else
                {
                    RotateSelfToPosition(stationaryLookAtPosition, 3f);
                    stationaryScanTimer = 0f;
                    currentState = GuardState.scanning;
                }

                break;

            case GuardState.scanning:
                if (stationaryScanTimer >= scanTime)
                {
                    stationaryScanTimer = 0f;
                    rightScanIsNext = !rightScanIsNext;
                    //Debug.Log("RightScanIsNext: " + rightScanIsNext);
                }
                else
                    stationaryScanTimer += Time.deltaTime;

                if (rightScanIsNext)
                {
                    float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, rightScanAngle.y, scanSpeed * Time.deltaTime);
                    transform.eulerAngles = new Vector3(0, angle, 0);
                }
                else
                {
                    float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, leftScanAngle.y, scanSpeed * Time.deltaTime);
                    transform.eulerAngles = new Vector3(0, angle, 0);
                }

                break;

            case GuardState.investigating:
                if (agent.remainingDistance <= investigateDestinationSpacing)
                    StartWaiting();

                break;
            case GuardState.dumbstruck: // While dumbstruck is active, the enemy will stand still and rotate towards the player until the dumbstruck timer runs out.
                if (dumbStateTimer < dumbstruckTime)
                {
                    dumbStateTimer += Time.deltaTime;
                    RotateSelfToPosition(PlayerDetectionPosition, 3f);
                }
                else if (detectingPlayer) // This runs once when enemy sees player at the end of dumbstruck time which transitions from 'dumbstruck' to 'chasing'.
                {
                    exclamationMark.SetActive(true);
                    audioSource.Stop();
                    audioSource.PlayOneShot(alertClip);
                    dumbStateTimer = TimerResetValue;
                    currentState = GuardState.chasing;
                    agent.speed = alertSpeed;
                    Debug.Log("Guard : " + guardNumber + " , Detected Player");
                }
                else // This runs once when enemy does not see the player at the end of dumbstruck time which transitions from 'dumbstruck' to 'waiting'.
                {
                    audioSource.PlayOneShot(mistakenClip);
                    dumbStateTimer = TimerResetValue;
                    StartWaiting();
                }

                break;
            case GuardState.chasing:
                if (!awareEnemies.Contains(gameObject))
                    awareEnemies.Add(gameObject);
                if (detectionTimer < lostDetectionDelay)
                {
                    agent.SetDestination(playerDetectionPoint.transform.position);
                    detectionTimer += Time.deltaTime;
                }

                if (detectingPlayer)
                    detectionTimer = TimerResetValue;
                else if (agent.remainingDistance <= chaseDestinationSpacing && detectionTimer >= lostDetectionDelay)
                {
                    if (awareEnemies.Contains(gameObject))
                        awareEnemies.Remove(gameObject);
                    exclamationMark.SetActive(false);
                    StartSearching(alertSpeed);
                }

                break;
            case GuardState.shooting:
                if (agent.remainingDistance >= 1f)
                    agent.SetDestination(transform.position);
                if (!awareEnemies.Contains(gameObject))
                    awareEnemies.Add(gameObject);
                break;

            case GuardState.searching:
                if (agent.remainingDistance <= 1f)
                    agent.SetDestination(GetRandomSearchPosition(searchPointRadius));
                if (searchTimer <= searchTime)
                    searchTimer += Time.deltaTime;
                else
                {
                    StartWaiting();
                    audioSource.PlayOneShot(lostplayerClip);
                }

                break;
            default:
                Debug.LogError("currentState is NULL!");
                break;
        }

        if (agent.velocity.magnitude >= 1f)
        {
            enemyAnim.SetMove(1);
        }
        else
            enemyAnim.SetMove(0);
    }

    /// <summary>
    /// Enemy starts searching in random directions.
    /// </summary>
    /// <param name="agentSpeed">Agent's speed while searching.</param>
    private void StartSearching(float agentSpeed)
    {
        searchTimer = TimerResetValue;
        currentState = GuardState.searching;
        agent.speed = agentSpeed;
        //// Enemy lost player sound
    }

    private void UpdateDetectionRays()
    {
        detectingPlayer = false;
        if (!isBlind && Physics.Raycast(headTransform.position, (playerDetectionPoint.transform.position - headTransform.position).normalized, out RaycastHit sightHit, detectionRange, detectionMask))
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
            detectionTimer = TimerResetValue;
        }
    }

    /// <summary>
    /// Returns a random NavMesh position within a set radius from the enemy.
    /// </summary>
    private Vector3 GetRandomSearchPosition(float searchRadius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
        randomDirection += transform.position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, 1);
        searchPointSet = true;
        return hit.position;
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
                // Since the order is now in reverse, we decrement to 1 index lower than the last index as our next waypoint.
                currentWaypointIndex = waypoints.Length - 2;
                isPathInverted = true;
            }
        }
        else
        {
            currentWaypointIndex--;

            if (currentWaypointIndex < 0)
            {
                // Since index 0 is already reached at this point, next index will be 1. (resetting to 0 will make the guard wait twice as long)
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

    private void OnTriggerStay(Collider other)
    {
        if (currentState == GuardState.patrolling || currentState == GuardState.waiting)
        {
            if (!isDeaf && other.gameObject.CompareTag("LuringSource"))
            {
                currentState = GuardState.investigating;
                agent.SetDestination(other.gameObject.transform.position);
            }

            if (!isDeaf && other.gameObject.CompareTag("RevealerSource"))
            {
                currentState = GuardState.chasing;
                ReactToPlayer();
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
}