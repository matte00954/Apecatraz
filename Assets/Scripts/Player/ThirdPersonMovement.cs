using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Main camera")]
    [SerializeField] private Transform mainCameraTransform;

    [Header("Controller")]
    [SerializeField] private CharacterController controller;
    private Animator animator;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Ledge")]
    [SerializeField] private LayerMask ledgeMask;
    [SerializeField] private GameObject ledgeCheck;
    private float ledgeCheckLength = 1.35f; 

    [Header("Energy")]
    [SerializeField] private Energy energy;
    private float teleportEnergyCost = 1f;

    //Changes during runtime
    private float turnSmoothVelocity;
    private bool inAir = false;
    private bool isTeleporting = false;
    private Vector3 velocity;

    //ground check
    private const float GroundCheckRadius = 0.15f; // comparing ground check game object to floor

    //Rotation
    private const float TurnSmoothTime = 0.1f;

    //Teleport
    private const float TeleportDistanceMultiplier = 0.15f; //per frame
    private const float TeleportDistanceCheck = 0.5f;
    private const float TeleportMarginMultiplier = 0.8f;

    //movement, these are constant
    private const float PlayerSpeed = 6f; //Do not change
    private const float JumpHeight = 4f; //Do not change

    //gravity
    private const float GravityValue = -9.81f; // do not change this -9.81f
    private const float GravityMultiplier = 1.5f; //multiplies gravity force

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined; //prevents mouse from leaving screen

        animator = GetComponentInChildren<Animator>();

        if (mainCameraTransform == null)
        {
            Debug.LogError("Camera not assigned to movement script, rotation will not work");
        }
        if (controller == null)
        {
            Debug.LogError("Controller not assigned to movement script, movement will not work");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        Movement();

        Gravity();

        Teleport();

        Ledge();

        if (Time.timeScale != 1 && !isTeleporting)
        {
            Time.timeScale = 1;
        }
    }

    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCameraTransform.eulerAngles.y; //first find target angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); //adjust angle for smoothing

            transform.rotation = Quaternion.Euler(0f, angle, 0f); //adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; //adjust direction to camera rotation/direction

            ControllerMove(moveDirection * PlayerSpeed * Time.deltaTime);
        }
        animator.SetFloat("runY", direction.magnitude); //Joches grej
    }

    private void Ledge() //may need to expand this, no bugs yet
    {
        RaycastHit hit;

        if (Physics.Raycast(ledgeCheck.gameObject.transform.position, Vector3.down, out hit, ledgeCheckLength))
        {
            velocity = new Vector3(0,0,0);
            controller.enabled = false;
            transform.position = hit.point;
            controller.enabled = true;
            Debug.Log("Ledge hit");
        }
        //may need another raycast to check front, works well without at the moment
    }

    private void Teleport()
    {
        if (Input.GetKey(KeyCode.LeftShift) && energy.CheckEnergy(teleportEnergyCost))
        {
            energy.SpendEnergy(1f);

            animator.SetTrigger("Teleport");

            isTeleporting = true;

            Time.timeScale = 0.4f;

            RaycastHit hit;

            if (Physics.SphereCast(transform.position, 1f, transform.forward, out hit, TeleportDistanceCheck))
            {
                ControllerMove(transform.forward * hit.distance * TeleportMarginMultiplier);
            }
            else
                ControllerMove(transform.forward * TeleportDistanceMultiplier);
        }
        else
        {
            isTeleporting = false;
        }
    }

    private void Gravity()
    {

        if (CheckGround() && Input.GetKeyDown(KeyCode.Space)) //Jump
        {
            animator.SetTrigger("Jump");
            inAir = true;
            velocity.y = Mathf.Sqrt(JumpHeight * -2f * GravityValue);
        }

        if (CheckGround() && velocity.y < 0)
        {
            velocity.y = -2f; //Default gravity force on the ground

            if (inAir)
            {
                animator.SetTrigger("Land");
                inAir = false;
            }
        }
        else
        {
            velocity.y += GravityValue * GravityMultiplier * Time.deltaTime; //gravity in the air

            if(inAir)
                animator.SetTrigger("InAir");

            if (!inAir)
                inAir = true;
        }

        ControllerMove(velocity * Time.deltaTime); //gravity applied
    }

    private void ControllerMove(Vector3 movement) //THIS IS THE ONLY controller.Move that should exist
    {
        controller.Move(movement);
    }

    private bool CheckGround()
    {
        return Physics.CheckSphere(groundCheck.position, GroundCheckRadius, groundMask);
    }

    /* OLD CLIMB FUNCTIONS
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Climb"))
        {
            Debug.Log("Climb trigger hit");
            Climb(other.gameObject.GetComponentInChildren<ClimbTransforms>());
        }
    }

    private void Climb(ClimbTransforms climbTransforms)
    {
        if (climbTransforms.climbList.Count == 0)
        {
            Debug.LogError("climbTransforms list is 0, should be atleast 1");
            return;
        }

        Vector3 closestPosition = transform.position;

        Vector3 currentPosition = transform.position;

        float minDistance = 0;

        for (int i = 0; i < climbTransforms.climbList.Count; i++)
        {
            if (i == 0)
            {
                minDistance = Vector3.Distance(currentPosition, climbTransforms.GetClimbPositionInList(i).position);
                closestPosition = climbTransforms.GetClimbPositionInList(i).position;
                Debug.Log("F�rsta siffran �r: " + minDistance);
            }

            if (minDistance > Vector3.Distance(currentPosition, climbTransforms.GetClimbPositionInList(i).position))
            {
                minDistance = Vector3.Distance(currentPosition, climbTransforms.GetClimbPositionInList(i).position);
                closestPosition = climbTransforms.GetClimbPositionInList(i).position;
                Debug.Log(i + " siffran �r: " + minDistance);
            }
        }
        velocity.y = 0;
        controller.enabled = false;
        transform.position = closestPosition;
        controller.enabled = true;
        */
}