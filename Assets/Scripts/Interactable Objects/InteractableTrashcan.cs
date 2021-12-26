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
        if (Input.GetKeyDown(KeyCode.Q))
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

/*[SerializeField]
private GameObject trashCanText;
private bool IsHiding = false;

[SerializeField]
private float cooldown = 1f;
private float cooldowncap;

private bool startCooldown = false;

private void Start()
{
    cooldowncap = cooldown;
}
private void Update()
{
    Debug.Log(cooldown);
    Debug.Log("cooldown cap" + cooldowncap);
    if (startCooldown == true)
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown = cooldowncap;
            startCooldown = false;
        }
    }
}
private void OnTriggerEnter(Collider other)
{
    if (other.gameObject.CompareTag("Player"))
    {
        trashCanText.SetActive(true);
    }
}

private void OnTriggerStay(Collider other)
{
    if (other.gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q) && IsHiding == false && cooldown >= cooldowncap)
    {
        SetActiveAllChildren(other.transform, false);
        other.gameObject.GetComponent<ThirdPersonMovement>().enabled = false;
        IsHiding = true;
        Debug.Log("set the player inactive");
        CooldownTimer();
    }
    else if (other.gameObject.CompareTag("Player") && Input.GetKeyDown(KeyCode.Q) && IsHiding == true && cooldown >= cooldowncap)
    {
        SetActiveAllChildren(other.transform, true);
        other.GetComponent<ThirdPersonMovement>().enabled = true;
        IsHiding = false;
        Debug.Log("set the player active");
        CooldownTimer();
    }
}
private void OnTriggerExit(Collider other)
{
    if (other.gameObject.CompareTag("Player"))
    {
        trashCanText.SetActive(false);
    }
}

private void SetActiveAllChildren(Transform transform, bool value)
{
    foreach (Transform child in transform)
    {
        child.gameObject.SetActive(value);
        SetActiveAllChildren(child, value);
    }
}

private void CooldownTimer()
{
    do
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown = cooldowncap;
        }
    } while (cooldown != cooldowncap);
}
}*/