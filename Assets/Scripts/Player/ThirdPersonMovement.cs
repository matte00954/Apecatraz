//Author: Mattias Larsson
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
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

    [Header("Ledge")]
    [SerializeField] private LayerMask ledgeMask;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject ledgeDownCheck;
    [SerializeField] private GameObject ledgeForwardCheck;
    [SerializeField] private AnimationClip climbAnimation;
    private RaycastHit ledgeHit;
    private bool startTimerForLedgeAnimation;
    private float ledgeCheckDownLength = 2f;
    private float ledgeCheckForwardLength = 2f;
    private float timeRemaining;
    private float maxClimbAnimationTimer;

    [Header("Energy")]
    [SerializeField] private Energy energy;
    private float dashEnergyCost = 5f;

    [Header("Ability Shaders")]
    [SerializeField] Material[] materials;
    private Renderer rend;

    [Header("ONLY FOR PROTOTYPES")]
    [SerializeField] private bool dashInactive = false;
    [SerializeField] private bool ledgeGrabInactive = false;
    [SerializeField] private bool telekinesInactive = false;
    [SerializeField] private bool godMode = false;
    [SerializeField] private bool infiniteEnergy = false;

    [HideInInspector] public bool isTelekinesisActive { get; set; }

    //Changes during runtime
    private float turnSmoothVelocity;
    private bool inAir = false;
    private bool moving = false;
    private Vector3 velocity;
    private float saveRotation;

    private DashEffects dashEffectsReference;

    private void Start()
    {
        if (telekinesInactive)
        {
            isTelekinesisActive = false;
        }
        else
            isTelekinesisActive = true;

        dashEffectsReference = mainCamera.GetComponent<DashEffects>();

        maxClimbAnimationTimer = climbAnimation.length;

        playerState = State.nothing;

        Cursor.lockState = CursorLockMode.Locked; //prevents mouse from leaving screen

        /////////////////////////////////////////////////////////////////
        //Emils grej
        rend = GetComponentInChildren<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[0];
        ////////////////////////////////////////////////////////////////

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

        if (!InGameMenuManager.gameIsPaused)
        {

            if (Time.timeScale != 1) //to unpause game
            {
                Time.timeScale = 1;
            }

            StateCheck();
        }

        if (InGameMenuManager.gameIsPaused && Cursor.lockState.Equals(CursorLockMode.Locked))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (!InGameMenuManager.gameIsPaused && Cursor.lockState.Equals(CursorLockMode.None))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

    private void StateCheck() //this is in update
    {
        switch (playerState)
        {
            case State.dashing:
                Dash();
                LedgeCheck();
                break;
            case State.telekinesis:
                Movement();
                Jump();
                break;
            case State.disabled:
                break;
            case State.climbing:
                break;
            case State.nothing:
                Movement();
                Jump();
                LedgeCheck();

                if (Input.GetKeyDown(KeyCode.LeftShift) && !dashInactive)
                {
                    playerState = State.dashing;
                    ActivateRenderer(1);
                }
                else
                    ActivateRenderer(0);

                break;
            default:
                Debug.LogError("Player state is null");
                break;
        }

        if(!playerState.Equals(State.climbing))
            Gravity();

        GetTurn();

        if (startTimerForLedgeAnimation)
        {
            LedgeAnimation();
        }
    }

    public void ActivateRenderer(int index)
    {
        rend.sharedMaterial = materials[index]; //To switch shaders when using ability
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

    private void LedgeCheck() 
    {
        if (!ledgeGrabInactive)
        {
            RaycastHit downHit; //ray from ledge check game object

            if (Physics.Raycast(ledgeDownCheck.gameObject.transform.position, new Vector3(0, ledgeCheckDownLength, 0), out downHit, ledgeCheckDownLength, ledgeMask)
                 && !playerState.Equals(State.telekinesis) && !playerState.Equals(State.climbing))
            {
                playerState = State.climbing;
                RaycastHit forwardHit;
                if (Physics.Raycast(ledgeForwardCheck.transform.position, transform.forward * ledgeCheckForwardLength, out forwardHit, ledgeCheckForwardLength, ~playerLayer))
                {
                    if (forwardHit.collider.gameObject.layer == ledgeMask)
                    {
                        animator.SetTrigger("LedgeGrab");
                        ledgeHit = downHit;
                        velocity = new Vector3(0, 0, 0); //removes all velocity during climb
                        controller.enabled = false;
                        startTimerForLedgeAnimation = true;
                        timeRemaining = maxClimbAnimationTimer;
                    }
                    else
                        Debug.LogError("ledgecheck number 2 hit something other than ledgemask " + forwardHit.collider.gameObject.layer);
                }
                else
                    Debug.LogError("ledgecheck number 2 did not hit anything ");
            }
        }
    }

    private void LedgeAnimation()
    {

        if (timeRemaining < 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            LedgeClimb();
            startTimerForLedgeAnimation = false;
        }
    }

    private void LedgeClimb()
    {
        MoveTo(ledgeHit.point);
        playerState = State.nothing;
    }

    private void Dash()
    {
        if (Input.GetKey(KeyCode.LeftShift) && energy.CheckEnergy(dashEnergyCost))
        {
            dashEffectsReference.SlowDown();
            ActivateRenderer(1); //Teleport shader

            energy.SpendEnergy(dashEnergyCost);

            //animator.SetTrigger("Teleport");


            RaycastHit hit;

            if (Physics.SphereCast(transform.position, 1f, transform.forward, out hit, TELEPORT_DISTANCE_CHECK, ledgeMask))
            {
                ControllerMove(transform.forward * hit.distance * DASH_MARIGIN_MULTIPLIER);
            }
            else
                ControllerMove(transform.forward * DASH_DISTANCE_MULTIPLIER);
        }
        else
        {
            ActivateRenderer(0);
            dashEffectsReference.SpeedUp();
            playerState = State.nothing;
        }
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

            /*if(inAir)
                animator.SetTrigger("InAir");*/

            if (!inAir)
                inAir = true;
        }

        ControllerMove(velocity * Time.deltaTime); //gravity applied
    }

    private void Jump()
    {
        if (CheckGround() && Input.GetKeyDown(KeyCode.Space)) //Jump
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
        if (controller.velocity.magnitude> 3)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(ledgeForwardCheck.transform.position, Vector3.forward * 2f);
    }
}