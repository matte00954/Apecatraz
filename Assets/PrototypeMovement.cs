using UnityEngine;

public class PrototypeMovement : MonoBehaviour
{


    public enum State { dashing, telekinesis, disabled, nothing, climbing }
    private State playerState;

    public State PlayerState { get => playerState; set => playerState = value; }

    //teleport
    private const float DASH_DISTANCE_MULTIPLIER = 0.75f; //per frame
    private const float TELEPORT_DISTANCE_CHECK = 0.5f;
    private const float DASH_MARIGIN_MULTIPLIER = 0.8f;

    //movement
    private const float PLAYER_SPEED = 6f; //Do not change
    private const float JUMP_HEIGHT = 4f; //Do not change

    //gravity
    private const float GRAVITY_VALUE = -9.81f; // do not change this -9.81f
    private const float GRAVITY_MULTIPLIER = 1.5f; //multiplies gravity force

    //ground check
    private const float GROUND_CHECK_RADIUS = 0.15f; // comparing ground check game object to floor

    //rotation
    private const float TURN_SMOOTH_TIME = 0.1f;

    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    //[SerializeField] private Transform mainCameraTransform;

    [Header("Controller")]
    [SerializeField] private CharacterController controller;
    private Animator animator;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    //Changes during runtime
    private float turnSmoothVelocity;
    private bool inAir = false;
    private bool moving = false;
    private Vector3 velocity;
    private float saveRotation;


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

    // Update is called once per frame
    private void Update()
    {
            StateCheck();
    }

    private void StateCheck() //this is in update
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
        
        Gravity();

        GetTurn();


    }


    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y; //first find target angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURN_SMOOTH_TIME); //adjust angle for smoothing
            transform.rotation = Quaternion.Euler(0f, angle, 0f); //adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; //adjust direction to camera rotation/direction

            ControllerMove(moveDirection * PLAYER_SPEED * Time.deltaTime);
        }
        if (CheckGround())
        {
            StopRunning();
        }
        animator.SetFloat("runY", direction.magnitude); //Joches grej
    }



    private void Gravity()
    {

        if (CheckGround() && velocity.y < 0) //On ground gravity
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
            velocity.y += GRAVITY_VALUE * GRAVITY_MULTIPLIER * Time.deltaTime; //gravity in the air
            animator.SetFloat("YSpeed", velocity.y);

            if (!inAir)
                inAir = true;
        }

        ControllerMove(velocity * Time.deltaTime); //gravity applied
    }

    private void Jump()
    {
        if (CheckGround() && Input.GetButtonDown("Jump"))//get("Jump")) //Jump
        {
            animator.SetTrigger("Jump");
            inAir = true;
            velocity.y = Mathf.Sqrt(JUMP_HEIGHT * -2f * GRAVITY_VALUE);
        }
    }

    private void ControllerMove(Vector3 movement) //THIS IS THE ONLY controller.Move that should exist
    {
        controller.Move(movement);
    }

    private bool CheckGround()
    {
        return Physics.CheckSphere(groundCheck.position, GROUND_CHECK_RADIUS, groundMask);
    }

    private void StopRunning() //Joche
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

    private void GetTurn() //Joche
    {
        animator.SetFloat("runX", (saveRotation + 1000) - (transform.eulerAngles.y + 1000));
        saveRotation = transform.eulerAngles.y;
    }

    public void MoveTo(Vector3 position)
    {
        controller.enabled = false;
        gameObject.transform.position = position;
        controller.enabled = true;
    }
}
