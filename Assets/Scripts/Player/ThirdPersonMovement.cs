//Author: Mattias Larsson
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    //Player States
    public enum State { dashing, telekinesis, disabled, nothing, climbing }
    private State playerState;

    public State PlayerState { get => playerState; set => playerState = value; }

    //teleport
    private const float DASH_DISTANCE_MULTIPLIER = 0.75f; //per frame
    private const float TELEPORT_DISTANCE_CHECK = 1f;
    private const float DASH_DISTANCE_CHECK = 1f;
    private const float DASH_MULTIPLIER = 50f;

    //movement
    private const float PLAYER_SPEED = 6f; //Do not change
    private const float JUMP_HEIGHT = 4f; //Do not change

    //dash
    private const float DASH_ENERGY_COST = 5f;

    //gravity
    private const float GRAVITY_VALUE = -9.81f; // do not change this -9.81f
    private const float GRAVITY_JUMP_APEX = -30f; //multiplies gravity force
    private const float LEDGE_CHECK_RAY_LENGTH_MULTIPLIER = 1.5f;

    //ground check
    private const float GROUND_CHECK_RADIUS = 0.15f; // comparing ground check game object to floor

    //rotation
    private const float TURN_SMOOTH_TIME = 0.1f;


    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField]private DashEffects dashEffectsReference;

    [Header("Controller")]
    [SerializeField] private CharacterController controller;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Ledge")]
    [SerializeField] private CharAnims charAnims;
    [SerializeField] private LayerMask ledgeMask;
    [SerializeField] private LayerMask dashIgnoreLayer;
    [SerializeField] private GameObject ledgeDownCheck;
    [SerializeField] private GameObject ledgeUpCheck;
    [SerializeField] private AnimationClip climbAnimation;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;


    [Header("Energy")]
    [SerializeField] private Energy energy;

    [Header("Ability Shaders")]
    [SerializeField] Material[] materials;
    private Renderer rend;

    [Header("ONLY FOR PROTOTYPES")]
    [SerializeField] private bool dashInactive = false;
    [SerializeField] private bool ledgeGrabInactive = false;
    [SerializeField] private bool telekinesInactive = false;
    [SerializeField] private bool godMode = false; //no effect atm
    [SerializeField] private bool slowmotionAllowed = false;

    [HideInInspector] public bool isTelekinesisActive { get; set; }

    //Changes during runtime
    private RaycastHit ledgeHit;
    private Vector3 velocity;
    private bool inAir = false;
    private bool isMoving = false;
    private float turnSmoothVelocity;
    private float dashCooldown;
    private float gravityTimer;
    private float timeRemainingOnAnimation;


    private void Start()
    {
        if (telekinesInactive)
        {
            isTelekinesisActive = false;
        }
        else
            isTelekinesisActive = true;

        playerState = State.nothing;

        Cursor.lockState = CursorLockMode.Locked; //prevents mouse from leaving screen

        #region Emil renderer
        /////////////////////////////////////////////////////////////////
        //Emils grej
        rend = GetComponentInChildren<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[0];
        ////////////////////////////////////////////////////////////////
        #endregion


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

            if (Time.timeScale != 1 && !slowmotionAllowed) //to unpause game
            {
                Time.timeScale = 1;
            }

            if (Input.GetKeyDown(KeyCode.Y) && godMode)
            {
                slowmotionAllowed = !slowmotionAllowed;
            }

            #region Joche slowmotion
            //ENDAST FÖR JOCHES PROTOTYP
            if (slowmotionAllowed)
            {

                if (PlayerState.Equals(State.dashing) && Time.timeScale != 0.2f) //ENDAST FÖR JOCHES PROTOTYP
                {
                    Time.timeScale = 0.2f;
                }
                else if (!PlayerState.Equals(State.dashing) && Time.timeScale != 1)
                {
                    Time.timeScale = 1f;
                }
            }
            //ENDAST FÖR JOCHES PROTOTYP
            #endregion

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
                DashCheck();
                break;
            case State.telekinesis:
                Movement();
                Jump();
                break;
            case State.disabled:
                break;
            case State.climbing:
                LedgeClimb();
                break;
            case State.nothing:
                Movement();
                Jump();
                LedgeCheck();
                DashCheck();
                break;
            default:
                Debug.LogError("Player state is null");
                break;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log(playerState.ToString() + " : dash cooldown : " + dashCooldown);
        }

        if (!playerState.Equals(State.dashing) && dashCooldown >= 0f)
            dashCooldown -= Time.deltaTime;

        if (!playerState.Equals(State.climbing))
            Gravity();

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
        charAnims.SetAnimFloat("runY", direction.magnitude); //Joches grej

        if (CheckGround())
        {
            charAnims.CheckStopRunning();
        }
    }

    #region Ledgeclimb
    private void LedgeCheck()
    {
        if (!ledgeGrabInactive)
        {
            RaycastHit upHit;

            if (Physics.Raycast(ledgeUpCheck.gameObject.transform.position, Vector3.up * LEDGE_CHECK_RAY_LENGTH_MULTIPLIER,
                out upHit, LEDGE_CHECK_RAY_LENGTH_MULTIPLIER)) //if player is above obstacle, do not climb
            {
                return;
            }
            else //nothing in the way
            {
                RaycastHit downHit; //ray from ledge check game object

                if (Physics.Raycast(ledgeDownCheck.gameObject.transform.position, Vector3.down * LEDGE_CHECK_RAY_LENGTH_MULTIPLIER,
                    out downHit, LEDGE_CHECK_RAY_LENGTH_MULTIPLIER, ledgeMask)) //checks if target surface has "climb" layer
                {
                    RaycastHit forwardHit;

                    if (Physics.Raycast(transform.position, transform.forward * LEDGE_CHECK_RAY_LENGTH_MULTIPLIER,
                        out forwardHit, LEDGE_CHECK_RAY_LENGTH_MULTIPLIER)) //checks distance from object so animation starts at correct the distance
                    {

                        MoveTo(new Vector3(forwardHit.point.x,
                            downHit.point.y - skinnedMeshRenderer.bounds.extents.y,
                            forwardHit.point.z));
                        //adjusts player position before animation
                        //y - y = height of object - height of player

                        playerState = State.climbing;

                        charAnims.SetTriggerFromString("LedgeGrab");

                        ledgeHit = downHit; //target position of climb

                        velocity = new Vector3(0, 0, 0); //removes all velocity during climb

                        timeRemainingOnAnimation = climbAnimation.length; 

                        //Method LedgeClimb() starts in update if playerstate is climbing
                    }
                }
            }
        }
    }

    private void LedgeClimb()
    {
        timeRemainingOnAnimation -= Time.deltaTime;

        if (timeRemainingOnAnimation < 0)
        {
            charAnims.SetTriggerFromString("StopClimb");

            MoveTo(ledgeHit.point);
            playerState = State.nothing;
        }
    }
    #endregion

    #region Dash
    private void DashCheck()
    {

        if (!dashInactive)
        {
            if (Input.GetKey(KeyCode.LeftShift) && dashCooldown <= 0f)
            {
                if (energy.CheckEnergy(DASH_ENERGY_COST))
                {
                    ActivateRenderer(1);
                    Dash();
                }
                else
                    StopDashing();
            }
            else if (playerState.Equals(State.dashing))
            {
                StopDashing();
            }
        }
    }

    private void Dash()
    {
        RaycastHit hit;

            if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out hit, DASH_DISTANCE_CHECK, ~dashIgnoreLayer))
            {
                StopDashing();
            }
            else
            {
                if (energy.CheckEnergy(DASH_ENERGY_COST))
                {
                    playerState = State.dashing;
                    dashEffectsReference.SlowDown();
                    ControllerMove(transform.forward * DASH_MULTIPLIER * Time.deltaTime);
                    energy.SpendEnergy(DASH_ENERGY_COST);
                }
            }
    }

    private void StopDashing()
    {
        ActivateRenderer(0);
        dashEffectsReference.SpeedUp();
        playerState = State.nothing;
        dashCooldown = 0.5f;
    }
    #endregion

    private void Gravity()
    {

        if (!CheckGround())
        {
            gravityTimer += Time.deltaTime;
        }

        if (CheckGround() && velocity.y < 0) //On ground gravity
        {
            velocity.y = -2f; //Default gravity force on the ground

            gravityTimer = 0f;

            if (inAir)
            {
                charAnims.SetTriggerFromString("Land");
                inAir = false;
            }
        }
        else
        {
            if(gravityTimer > 0.65f)
            {
                velocity.y += GRAVITY_JUMP_APEX * Time.deltaTime;
            }
            else
                velocity.y += GRAVITY_VALUE * Time.deltaTime;

            charAnims.SetAnimFloat("YSpeed", velocity.y);

            if (!inAir)
                inAir = true;
        }

        ControllerMove(velocity * Time.deltaTime); //gravity applied
    }

    private void Jump()
    {
        if (CheckGround() && Input.GetKeyDown(KeyCode.Space)) //Jump
        {
            charAnims.SetTriggerFromString("Jump");
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

    public void MoveTo(Vector3 position)
    {
        controller.enabled = false;
        gameObject.transform.position = position;
        controller.enabled = true;
    }

    public void ActivateRenderer(int index)
    {
        rend.sharedMaterial = materials[index]; //To switch shaders when using ability
    }

    public float GetVelocity()
    {
        return controller.velocity.magnitude;
    }

    public State GetState()
    {
        return playerState;
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }

    public void SetIsMoving(bool newMoveBool)
    {
        isMoving = newMoveBool;
    }
}

/*private void StopRunning() //Joche
{
    if (controller.velocity.magnitude > 3)
    {
        if (!isMoving)
        {
            animator.SetTrigger("Start");
        }
        isMoving = true;
    }
    if (controller.velocity.magnitude < 3 && isMoving)
    {
        isMoving = false;
        animator.SetTrigger("Stop");
    }
}*/
