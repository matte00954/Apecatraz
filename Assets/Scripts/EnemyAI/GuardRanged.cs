//Author: William Örnquist
using UnityEngine;

public class GuardRanged : MonoBehaviour
{
    [Header("References")]
    private EnemyMovement enemyMovement;
    [Header("Chase state variables")]
    [SerializeField, Range(5f, 50f), Tooltip("The maximum range from the player before the enemy can start shooting.")]
    private float attackRangeMax = 20f;

    private void Awake() => enemyMovement = GetComponent<EnemyMovement>();

    void Update()
    {
        if (Vector3.Distance(enemyMovement.HeadTransform.position, enemyMovement.PlayerDetectionPoint.transform.position) <= attackRangeMax)
        {

        }
    }
}