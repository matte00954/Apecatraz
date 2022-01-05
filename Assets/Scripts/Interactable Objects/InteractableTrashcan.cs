// Author: [Jacob Wik]
using UnityEngine;

public class InteractableTrashcan : MonoBehaviour
{
    private const string TrashcanTagName = "Trashcan";

    [SerializeField] private GameObject trashCanTextUI;
    [SerializeField] private GameObject trashCanIsChasedUI;
    [SerializeField] private ThirdPersonMovement thirdPersonMovement;

    private Vector3 cachedEnteredLocation;
    private bool canEnter = false;
    private float chasedTimer = 3f;
    private Transform targetTransform;

    private void Start()
    {
        if (trashCanTextUI.activeInHierarchy)
            trashCanTextUI.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire4"))
        {
            if (canEnter && thirdPersonMovement.PlayerState.Equals(ThirdPersonMovement.State.nothing) && EnemyMovement.AwareEnemies.Count == 0)
                EnterTrashCan();
            else if (thirdPersonMovement.PlayerState.Equals(ThirdPersonMovement.State.hiding))
                ExitTrashCan();
            else if (canEnter)
                trashCanIsChasedUI.SetActive(true);
        }

        if (trashCanIsChasedUI.activeSelf)
        {
            chasedTimer -= Time.deltaTime;

            if (chasedTimer < 0)
            {
                trashCanIsChasedUI.SetActive(false);
                chasedTimer = 3f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(TrashcanTagName))
        {
            trashCanTextUI.SetActive(true);
            canEnter = true;

            targetTransform = other.gameObject.GetComponent<TrashcanTransform>().GetEnterLocation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(TrashcanTagName))
        {
            trashCanTextUI.SetActive(false);
            canEnter = false;
        }
    }

    private void EnterTrashCan()
    {
        cachedEnteredLocation = transform.position;
        thirdPersonMovement.PlayerState = ThirdPersonMovement.State.hiding;
        thirdPersonMovement.MoveTo(targetTransform.position);
        thirdPersonMovement.ToggleRigidbodyCollisions(false);
    }

    private void ExitTrashCan()
    {
        thirdPersonMovement.PlayerState = ThirdPersonMovement.State.nothing;
        thirdPersonMovement.MoveTo(cachedEnteredLocation);
        thirdPersonMovement.ToggleRigidbodyCollisions(true);
    }
}
