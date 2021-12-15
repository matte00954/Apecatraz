// Author: William �rnquist
using UnityEngine;

public class GuardRanged : MonoBehaviour
{
    [Header("References")]
    private EnemyMovement enemyMovement;
    private EnemyAnims enemyAnims;
    [SerializeField] private GameObject netProjectilePrefab;
    [SerializeField] private GameObject firePositionObject;
    [SerializeField] private GameObject playerMovementPredictionObject;

    [Header("Sounds")]
    [SerializeField] private AudioClip chargingClip;
    [SerializeField] private AudioClip shootClip;
    private AudioSource audioSource;

    [Header("Chase state variables")]
    [SerializeField, Range(5f, 50f), Tooltip("The maximum range from the player before the enemy can start shooting.")]
    private float attackRangeMax = 15f;
    [SerializeField, Range(0f, 5f), Tooltip("The amount of time the enemy has to stay still and charge the weapon before shooting at the player.")]
    private float attackChargeTime = 1f;

    private float chargeTimer;
    private bool isCharging;
    private bool isInRange;

    public void ResetGuard()
    {
        isCharging = false;
        chargeTimer = attackChargeTime;
        enemyAnims.StopAiming();
        enemyMovement.ResetTransform();
        enemyMovement.StartWaiting();
    }

    private void Awake() 
    {
        chargeTimer = attackChargeTime;
        enemyMovement = GetComponent<EnemyMovement>();
        enemyAnims = GetComponentInChildren<EnemyAnims>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameManager.Guards.Add(gameObject);
    }

    private void Update()
    {
        // Starts charging the weapon when the following conditions are meet.
        if (Vector3.Distance(enemyMovement.HeadPosition, enemyMovement.PlayerDetectionPosition) <= attackRangeMax
            && enemyMovement.IsDetectingPlayer && enemyMovement.CurrentState != EnemyMovement.GuardState.dumbstruck)
        {
            if (!isCharging && chargeTimer >= attackChargeTime)
            {
                chargeTimer = 0f;
                enemyMovement.CurrentState = EnemyMovement.GuardState.shooting;
                isCharging = true;
                audioSource.PlayOneShot(chargingClip);
                enemyAnims.Aim();
            }

            isInRange = true;
        }
        else if (isInRange)
            isInRange = false;

        // Rotates towards the player while counting down the charge timer.
        if (isCharging && chargeTimer < attackChargeTime) 
        {
            enemyMovement.RotateSelfToPlayer();
            chargeTimer += Time.deltaTime;
        }
        else if (isCharging && chargeTimer >= attackChargeTime) 
        {
            isCharging = false;
            FireAtPlayer();
            enemyMovement.RefreshDetectionDelay();
            if (!isInRange || !enemyMovement.IsDetectingPlayer)
            {
                enemyMovement.Agent.SetDestination(enemyMovement.PlayerDetectionPosition);
                enemyMovement.CurrentState = EnemyMovement.GuardState.chasing;
                enemyAnims.StopAiming();
            }
        }
    }

    private void FireAtPlayer()
    {
        firePositionObject.transform.LookAt(playerMovementPredictionObject.transform.position);
        Instantiate(netProjectilePrefab, firePositionObject.transform.position, firePositionObject.transform.rotation);
        audioSource.PlayOneShot(shootClip);
        enemyAnims.Fire();
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