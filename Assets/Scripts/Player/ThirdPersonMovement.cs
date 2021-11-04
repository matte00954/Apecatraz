//Author: Mattias Larsson
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    public enum State { dashing, telekinesis, disabled, nothing }
    private State playerState;

    public State PlayerState { get => playerState; set => playerState = value; }

    /*
     *     private const float MIN_SPACING_PATROL = 1f;
    private const float MIN_SPACING_INVESTIGATE = 10f;
    private const float MIN_SPACING_CHASE = 5f;
     */

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
    [SerializeField] private GameObject ledgeCheck;
    [SerializeField] private AnimationClip climbAnimation;
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    private Vector3 betweenFeet;
    private RaycastHit ledgeHit;
    private bool startTimerForLedgeAnimation;
    private float ledgeCheckDownLength = 1.35f;
    private float ledgeCheckForward = 10f;
    private float timeRemaining;
    private float maxClimbAnimationTimer;

    [Header("Energy")]
    [SerializeField] private Energy energy;
    private float dashEnergyCost = 5f;

    [Header("Ability Shaders")]
    [SerializeField] Material[] materials;
    private Renderer rend;

    //Changes during runtime
    private float turnSmoothVelocity;
    private bool inAir = false;
    private bool moving = false;
    private Vector3 velocity;
    private float saveRot;

    //ground check
    private const float GroundCheckRadius = 0.15f; // comparing ground check game object to floor

    //Rotation
    private const float TurnSmoothTime = 0.1f;

    //Teleport
    private const float DashDistanceMultiplier = 0.75f; //per frame
    private const float TeleportDistanceCheck = 0.5f;
    private const float DashMarginMultiplier = 0.8f;

    //movement, these are constant
    private const float PlayerSpeed = 6f; //Do not change
    private const float JumpHeight = 4f; //Do not change

    //gravity
    private const float GravityValue = -9.81f; // do not change this -9.81f
    private const float GravityMultiplier = 1.5f; //multiplies gravity force

    private DashEffects dashEffectsReference;

    private void Start()
    {
        betweenFeet = rightFoot.position + leftFoot.position / 2;

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
        GetTurn();
        if (!InGameMenuManager.gameIsPaused)
        {
            if (Time.timeScale != 1) //to unpause game
            {
                ActivateRenderer(0); //Default shader

                Time.timeScale = 1;
            }

            StateCheck();

            if (startTimerForLedgeAnimation)
            {
                LedgeAnimation();
            }

            if (Input.GetKey(KeyCode.P))
            {
                ResetScene.RestartScene();
            }

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

    private void StateCheck()
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
            case State.nothing:
                Movement();
                Jump();
                LedgeCheck();

                if (Input.GetKeyDown(KeyCode.LeftShift))
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

        Gravity(); //Always on
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
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); //adjust angle for smoothing
            transform.rotation = Quaternion.Euler(0f, angle, 0f); //adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; //adjust direction to camera rotation/direction

            ControllerMove(moveDirection * PlayerSpeed * Time.deltaTime);
        }
        if (CheckGround())
        {
            StopRunning();
        }
        animator.SetFloat("runY", direction.magnitude); //Joches grej
    }

    private void LedgeCheck() //may need to expand this, no bugs yet
    {
        RaycastHit downHit; //ray from ledge check game object

        if (Physics.Raycast(ledgeCheck.gameObject.transform.position, Vector3.down, out downHit, ledgeCheckDownLength, ledgeMask)
            && !playerState.Equals(State.dashing) && !playerState.Equals(State.telekinesis))
        {
            RaycastHit forwardHit;
            if (Physics.Raycast(transform.position, Vector3.forward, out forwardHit, ledgeCheckForward, ledgeMask))
            {
                animator.SetTrigger("LedgeGrab");
                ledgeHit = downHit;
                Debug.Log("CLIMB");
                playerState = State.disabled;
                velocity = new Vector3(0, 0, 0); //removes all velocity during climb
                controller.enabled = false;
                timeRemaining = maxClimbAnimationTimer;
                startTimerForLedgeAnimation = true;
            }
        }
    }

    private void LedgeAnimation()
    {

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }

        if (timeRemaining < maxClimbAnimationTimer)
        {
            startTimerForLedgeAnimation = false;
            LedgeClimb();
        }
    }

    private void LedgeClimb()
    {
        transform.position = ledgeHit.point;
        controller.enabled = true;
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

            if (Physics.SphereCast(transform.position, 1f, transform.forward, out hit, TeleportDistanceCheck, ledgeMask))
            {
                ControllerMove(transform.forward * hit.distance * DashMarginMultiplier);
            }
            else
                ControllerMove(transform.forward * DashDistanceMultiplier);
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
            velocity.y += GravityValue * GravityMultiplier * Time.deltaTime; //gravity in the air
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
            velocity.y = Mathf.Sqrt(JumpHeight * -2f * GravityValue);
        }
    }

    private void ControllerMove(Vector3 movement) //THIS IS THE ONLY controller.Move that should exist
    {
        controller.Move(movement);
    }

    private bool CheckGround()
    {
        return Physics.CheckSphere(groundCheck.position, GroundCheckRadius, groundMask);
    }

    private void StopRunning()
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

    private void GetTurn()
    {
        animator.SetFloat("runX", (saveRot + 1000) - (transform.eulerAngles.y + 1000));
        saveRot = transform.eulerAngles.y;
    }
}