using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField, Tooltip("An array of waypoint objects the guard should follow in order.")]
    private GameObject[] waypoints;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField, Tooltip("A stationary guard will stay and move back to one spot.")]
    private bool isStationary;
    [SerializeField, Tooltip("Check this if this guard follows an encircling path.")]
    private bool isCircling;
    [SerializeField, Tooltip("The amount of time the guard should wait after reaching a waypoint.")]
    private float totalWaitTime;

    private int currentWaypointIndex;
    private float waitTimer;
    private bool isAlerted;
    private bool isPathInverted;

    private enum GuardState { patrolling, waiting, investigating, dumbstruck, chasing, alarming }
    private GuardState currentState;

    public void Start()
    {
        if (isStationary)
            Debug.Log(gameObject.name + " is set to stationary.");
        else if (agent == null)
            Debug.LogError("The nav mesh agent component is not attached to " + gameObject.name + ".");
        else if (waypoints == null || waypoints.Length <= 1)
            Debug.LogError("Insufficient waypoints for basic patrolling behaviour.");
        else
        {
            currentWaypointIndex = 0;
            currentState = GuardState.patrolling;
        }
    }

    void Update()
    {
        if (currentState == GuardState.patrolling && agent.remainingDistance <= 1f
            || currentState == GuardState.investigating && agent.remainingDistance <= 4f)
        {
            currentState = GuardState.waiting;
            waitTimer = 0f;
        }

        if (currentState == GuardState.waiting)
        {
            if (waitTimer < totalWaitTime)
                waitTimer += Time.deltaTime;
            else
            {
                waitTimer = 0f;
                currentState = GuardState.patrolling;

                NextWaypoint();
                ResumePatrol();
            }
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
        Debug.Log("collision detected");
        if (currentState == GuardState.patrolling || currentState == GuardState.waiting)
        {
            if (other.gameObject.CompareTag("LuringSource"))
            {
                currentState = GuardState.investigating;
                agent.SetDestination(other.gameObject.transform.position);
            }
        }
    }
}