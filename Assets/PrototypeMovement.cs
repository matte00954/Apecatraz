// Author: [full name here]
using UnityEngine;

public class PrototypeMovement : MonoBehaviour
{
    // Teleport
    private const float DashDistanceMultiplier = 0.75f; // Per frame
    private const float TeleportDistanceCheck = 0.5f;
    private const float DashMarginMultiplier = 0.8f;

    // Movement
    private const float PlayerSpeed = 6f; // Do not change
    private const float JumpHeight = 4f; // Do not change

    // Gravity
    private const float GravityValue = -9.81f; // Do not change this -9.81f
    private const float GravityMultiplier = 1.5f; // Multiplies gravity force

    // Ground check
    private const float GroundCheckRadius = 0.15f; // Comparing ground check game object to floor

    // Rotation
    private const float TurnSmoothTime = 0.1f;

    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    ////[SerializeField] private Transform mainCameraTransform;

    [Header("Controller")]
    [SerializeField] private CharacterController controller;
    private Animator animator;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    // Changes during runtime
    private float turnSmoothVelocity;
    private bool inAir = false;
    private bool moving = false;
    private Vector3 velocity;
    private float saveRotation;

    private State playerState;

    public enum State { dashing, telekinesis, disabled, nothing, climbing }
    public State PlayerState { get => playerState; set => playerState = value; }

    public void MoveTo(Vector3 position)
    {
        controller.enabled = false;
        gameObject.transform.position = position;
        controller.enabled = true;
    }

    private void Start()
    {
        playerState = State.nothing;
        animator = GetComponentInChildren<Animator>();

        if (mainCamera.transform == null)
        {
            Debug.LogError("Camera not assigned to movement script, rotation will not work");
        }

        if (controller == null)
        {
            Debug.LogError("Controller not assigned to movement script, movement will not work");
        }
    }

    private void Update()
    {
        StateCheck();
    }

    private void StateCheck()
    {
        switch (playerState)
        {
            case State.nothing:
                Movement();
                Jump();
                break;
            default:
                Debug.LogError("Player state is null");
                break;
        }

        ApplyGravity();
        GetTurn();
    }

    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + mainCamera.transform.eulerAngles.y; // First find target angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); // Adjusts angle for smoothing
            transform.rotation = Quaternion.Euler(0f, angle, 0f); // Adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // Adjusts direction to camera rotation/direction

            ControllerMove(moveDirection * PlayerSpeed * Time.deltaTime);
        }

        if (CheckGround())
            StopRunning();

        animator.SetFloat("runY", direction.magnitude); // Added by Joche
    }

    private void ApplyGravity()
    {
        // On ground gravity
        if (CheckGround() && velocity.y < 0) 
        {
            velocity.y = -2f; // Default gravity force on the ground

            if (inAir)
            {
                animator.SetTrigger("Land");
                inAir = false;
            }
        }
        else
        {
            velocity.y += GravityValue * GravityMultiplier * Time.deltaTime; // Gravity in the air
            animator.SetFloat("YSpeed", velocity.y);

            if (!inAir)
                inAir = true;
        }

        ControllerMove(velocity * Time.deltaTime); // Gravity applied
    }

    private void Jump()
    {
        if (CheckGround() && Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("Jump");
            inAir = true;
            velocity.y = Mathf.Sqrt(JumpHeight * -2f * GravityValue);
        }
    }

    private void ControllerMove(Vector3 movement) // THIS IS THE ONLY controller.Move that should exist
    {
        controller.Move(movement);
    }

    private bool CheckGround()
    {
        return Physics.CheckSphere(groundCheck.position, GroundCheckRadius, groundMask);
    }

    private void StopRunning() // Added by Joche
    {
        if (controller.velocity.magnitude > 3)
        {
            if (!moving)
            {
                animator.SetTrigger("Start");
            }

            moving = true;
        }

        if (controller.velocity.magnitude < 3 && moving)
        {
            moving = false;
            animator.SetTrigger("Stop");
        }
    }

    private void GetTurn() // Added by Joche
    {
        animator.SetFloat("runX", (saveRotation + 1000) - (transform.eulerAngles.y + 1000));
        saveRotation = transform.eulerAngles.y;
    }
}
