//Author: William �rnquist
using UnityEngine;

public class GuardRanged : MonoBehaviour
{
    [Header("References")]
    private EnemyMovement enemyMovement;
    [SerializeField] private GameObject netProjectilePrefab;
    [SerializeField] private GameObject firePositionObject;
    [SerializeField] private GameObject playerMovementPredictionObject;


    [Header("Chase state variables")]
    [SerializeField, Range(5f, 50f), Tooltip("The maximum range from the player before the enemy can start shooting.")]
    private float attackRangeMax = 15f;
    [SerializeField, Range(0f, 5f), Tooltip("The amount of time the enemy has to stay still and charge the weapon before shooting at the player.")]
    private float attackChargeTime = 1f;

    private float chargeTimer;
    private bool isCharging;

    private void Awake() 
    {
        chargeTimer = attackChargeTime;
        enemyMovement = GetComponent<EnemyMovement>();
    } 

    void Update()
    {
        if (Vector3.Distance(enemyMovement.HeadPosition, enemyMovement.PlayerDetectionPosition) <= attackRangeMax
            && enemyMovement.DetectingPlayer && chargeTimer >= attackChargeTime && !isCharging
            && enemyMovement.CurrentState != EnemyMovement.GuardState.dumbstruck)
        {
            chargeTimer = 0f;
            enemyMovement.CurrentState = EnemyMovement.GuardState.shooting;
            isCharging = true;
        }

        if (isCharging && chargeTimer < attackChargeTime)
        {
            enemyMovement.RotateSelfToPlayer();
            chargeTimer += Time.deltaTime;
        }
        else if (isCharging && chargeTimer >= attackChargeTime)
        {
            isCharging = false;
            FireAtPlayer();
            enemyMovement.Agent.SetDestination(enemyMovement.PlayerDetectionPosition);
            enemyMovement.RefreshDetectionDelay();
            enemyMovement.CurrentState = EnemyMovement.GuardState.chasing;
        }
    }

    private void FireAtPlayer()
    {
        firePositionObject.transform.LookAt(playerMovementPredictionObject.transform.position);
        Instantiate(netProjectilePrefab, firePositionObject.transform.position, firePositionObject.transform.rotation);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (enemyMovement.PlayerDetectionPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(enemyMovement.HeadPosition, (enemyMovement.PlayerDetectionPosition - enemyMovement.HeadPosition).normalized * attackRangeMax);
            }
        }
    }
}