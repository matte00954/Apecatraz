//Author: Mattias Larsson
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    //Player States
    public enum State { dashing, telekinesis, disabled, nothing, climbing }
    private State playerState;

    public State PlayerState { get => playerState; set => playerState = value; }

    //teleport
    //private const float DASH_DISTANCE_MULTIPLIER = 0.75f; //per frame
    //private const float TELEPORT_DISTANCE_CHECK = 1f;
    private const float DASH_DISTANCE_CHECK = 1f;
    private const float DASH_FORCE = 50f;

    //movement
    private const float START_PLAYER_SPEED = 250f; //Do not change
    private const float MAX_PLAYER_SPEED = 500f; //Do not change
    private const float JUMP_HEIGHT = 17.5f; //Do not change
    private const float JUMP_FORWARD_FORCE = 10f; //Do not change

    //dash
    private const float DASH_ENERGY_COST = 5f;

    //gravity
    private const float GRAVITY_VALUE = 20f;
    private const float GRAVITY_JUMP_APEX = 40f;
    private const float LEDGE_CHECK_RAY_LENGTH_MULTIPLIER = 1.5f;

    //ground check
    private const float GROUND_CHECK_RADIUS = 0.10f; // comparing ground check game object to floor

    //rotation
    private const float TURN_SMOOTH_TIME = 0.1f;
    private const float TURN_SMOOTH_TIME_IN_AIR = 0.4f;


    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DashEffects dashEffectsReference;

    [Header("Controller")]
    //[SerializeField] private CharacterController controller;
    [SerializeField] private Rigidbody rb;

    [Header("Ground check")]
    [SerializeField] private Transform frontFeetGroundCheck;
    [SerializeField] private Transform backFeetGroundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Ledge")]
    [SerializeField] private CharAnims charAnims;
    [SerializeField] private LayerMask ledgeMask;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GameObject ledgeDownCheck;
    [SerializeField] private GameObject ledgeUpCheck;
    [SerializeField] private AnimationClip climbAnimation;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private Collider playerCollider;

    [Header("Energy")]
    [SerializeField] private Energy energy;

    [Header("Head raycast origin")]
    [SerializeField] private Transform headRaycastOrigin;

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


    //Rigidbody
    private float defaultDrag;
    private float defaultAngularDrag;

    //Changes during runtime
    private RaycastHit ledgeHit;
    private bool inAir = false;
    private bool isMoving = false;
    private float playerSpeed;
    private float turnSmoothVelocity;
    private float dashCooldown;
    private float dashTimer;
    private float timeRemainingOnAnimation;

    // INPUT NEEDS TO BE IN UPDATE, MOVEMENT IN FIXED
    // INPUT NEEDS TO BE IN UPDATE, MOVEMENT IN FIXED
    // INPUT NEEDS TO BE IN UPDATE, MOVEMENT IN FIXED
    // INPUT NEEDS TO BE IN UPDATE, MOVEMENT IN FIXED

    //ALL CLIMBABLE OBJECTS NEED A TRIGGER WITH CLIMB LAYER

    private void Start()
    {
        defaultDrag = rb.drag;
        defaultAngularDrag = rb.angularDrag;

        dashTimer = 0.2f;

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
        /*if (controller == null)
        {
            Debug.LogError("Controller not assigned to movement script, movement will not work");
        }*/
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
            //ENDAST F�R JOCHES PROTOTYP
            if (slowmotionAllowed)
            {

                if (PlayerState.Equals(State.dashing) && Time.timeScale != 0.2f) //ENDAST F�R JOCHES PROTOTYP
                {
                    Time.timeScale = 0.2f;
                }
                else if (!PlayerState.Equals(State.dashing) && Time.timeScale != 1)
                {
                    Time.timeScale = 1f;
                }
            }
            //ENDAST F�R JOCHES PROTOTYP
            #endregion
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

    private void FixedUpdate()
    {
        if (!InGameMenuManager.gameIsPaused)
        {
            StateCheck();
        }
    }

    private void StateCheck() //this is in update
    {
        switch (playerState)
        {
            case State.dashing:
                dashTimer -= Time.fixedDeltaTime;
                DashCheck();
                break;
            case State.telekinesis:
                Movement();
                break;
            case State.disabled: // disabled = captured/d�d
                //spela death anim
                //reset spel
                break;
            case State.climbing:
                LedgeClimb();
                break;
            case State.nothing:
                Movement();
                LedgeCheck();
                DashCheck();
                break;
            default:
                Debug.LogError("Player state is null");
                break;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
        }

        if (!playerState.Equals(State.dashing))
            if(dashCooldown >= 0f)
                dashCooldown -= Time.fixedDeltaTime;

        if (!playerState.Equals(State.dashing) && !playerState.Equals(State.climbing) && !rb.useGravity)
        {
            rb.useGravity = true;
        }

        /*if (!playerState.Equals(State.climbing) && !playerState.Equals(State.dashing))
        {
            rb.useGravity = false;

        }
        else
        {
            rb.useGravity = true;
        }*/

        /*if (!playerState.Equals(State.dashing) && !playerState.Equals(State.climbing))
            Gravity();*/

    }

    private void Movement()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical);

        if (horizontal == 0 && vertical == 0)
        {
            playerSpeed = START_PLAYER_SPEED;
        }

        if (direction.magnitude >= 0.1f)
        {
            if (playerSpeed < MAX_PLAYER_SPEED)
            {
                playerSpeed += Time.fixedDeltaTime * 200f;
            }
            else if (playerSpeed > MAX_PLAYER_SPEED)
            {
                playerSpeed = MAX_PLAYER_SPEED;
            }

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y; //first find target angle
            float angle;

            if (inAir)
            {
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURN_SMOOTH_TIME_IN_AIR); //adjust angle for smoothing
            }
            else
            {
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURN_SMOOTH_TIME); //adjust angle for smoothing
            }

            transform.rotation = Quaternion.Euler(0f, angle, 0f); //adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; //adjust direction to camera rotation/direction

            if (horizontal != 0 || vertical != 0 &&
                !Physics.Raycast(headRaycastOrigin.position, headRaycastOrigin.transform.forward * 0.2f, 0.25f, ~playerLayer)) //cant add velocity if something is in the way
            {
                if (CheckGround(frontFeetGroundCheck) || CheckGround(backFeetGroundCheck))
                {
                    if (rb.velocity.z < MAX_PLAYER_SPEED || rb.velocity.x < MAX_PLAYER_SPEED)
                        rb.velocity = playerSpeed * Time.fixedDeltaTime * moveDirection; //On ground
                }
                else
                    rb.AddForce(MAX_PLAYER_SPEED * Time.fixedDeltaTime * moveDirection); //In air
            }
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        if (Input.GetKeyDown(KeyCode.Space)) //Jump
        {
            if (CheckGround(backFeetGroundCheck))
            {
                charAnims.SetTriggerFromString("Jump");
                inAir = true;
                rb.AddForce(0, JUMP_HEIGHT, 0, ForceMode.Impulse);
                if (!Physics.Raycast(headRaycastOrigin.position, transform.forward, 1.5f, ~playerLayer))
                {
                    rb.AddForce(transform.forward * JUMP_FORWARD_FORCE, ForceMode.Impulse);
                }
            }
        }

        //gravity
        if (CheckGround(frontFeetGroundCheck) || CheckGround(backFeetGroundCheck)) //On ground gravity
        {

            if (inAir)
            {
                charAnims.SetTriggerFromString("Land");
                inAir = false;
            }
        }
        else //In air
        {
            if (rb.velocity.y > 0f)
            {
                rb.AddForce(Vector3.down * GRAVITY_JUMP_APEX);
            }
            else
                rb.AddForce(Vector3.down * GRAVITY_VALUE);

            charAnims.SetAnimFloat("YSpeed", rb.velocity.y);

            if (!inAir)
                inAir = true;
        }

        charAnims.SetAnimFloat("runY", direction.magnitude); //Joches grej

        if (CheckGround(frontFeetGroundCheck) || CheckGround(backFeetGroundCheck))
        {
            charAnims.CheckStopRunning();
        }
    }

    private void PlayerAcceleration()
    {
        playerSpeed += Mathf.MoveTowards(START_PLAYER_SPEED, MAX_PLAYER_SPEED, 50f);
    }

    private void CheckFloorRotation()
    {
        RaycastHit floorRotation;

        if (Physics.Raycast(backFeetGroundCheck.transform.position, Vector3.down * 1f,
                   out floorRotation, 1f, groundMask)) //Uses ledge check ray var, because length works here too
        {
            if (floorRotation.transform.rotation != Quaternion.identity)
            {
                rb.freezeRotation = false;
                rb.rotation = floorRotation.transform.rotation;
                rb.freezeRotation = true;
            }
        }
    }

    private void ResetDrag() //probably not needed
    {
        if (rb.angularDrag != defaultAngularDrag)
        {
            rb.angularDrag = defaultAngularDrag;
        }
        if (rb.drag != defaultDrag)
        {
            rb.drag = defaultDrag;
        }
    }

    #region Ledgeclimb
    private void LedgeCheck()
    {
        if (!ledgeGrabInactive)
        {
            RaycastHit upHit;

            if (Physics.Raycast(ledgeUpCheck.transform.position, Vector3.up * LEDGE_CHECK_RAY_LENGTH_MULTIPLIER,
                out upHit, LEDGE_CHECK_RAY_LENGTH_MULTIPLIER)) //if player is above obstacle, do not climb
            {
                return;
            }
            else //nothing in the way
            {
                RaycastHit downHit; //ray from ledge check game object

                if (Physics.Raycast(ledgeDownCheck.transform.position, Vector3.down * LEDGE_CHECK_RAY_LENGTH_MULTIPLIER,
                    out downHit, LEDGE_CHECK_RAY_LENGTH_MULTIPLIER, ledgeMask)) //checks if target surface has "climb" layer
                {
                    RaycastHit forwardHit;

                    if (Physics.Raycast(frontFeetGroundCheck.transform.position, transform.forward * LEDGE_CHECK_RAY_LENGTH_MULTIPLIER,
                        out forwardHit, LEDGE_CHECK_RAY_LENGTH_MULTIPLIER)) //checks distance from object so animation starts at correct the distance
                    {
                        rb.useGravity = false;

                        playerCollider.enabled = false;

                        rb.velocity = Vector3.zero;

                        MoveTo(new Vector3(forwardHit.point.x,
                            downHit.point.y - skinnedMeshRenderer.bounds.extents.y,
                            forwardHit.point.z));
                        //adjusts player position before animation
                        //y - y = height of object - height of player

                        playerState = State.climbing;

                        charAnims.SetTriggerFromString("LedgeGrab");

                        ledgeHit = downHit; //target position of climb

                        timeRemainingOnAnimation = climbAnimation.length;

                        //Method LedgeClimb() starts in update if playerstate is climbing
                    }
                }
            }
        }
    }

    private void LedgeClimb()
    {
        timeRemainingOnAnimation -= Time.fixedDeltaTime;

        if (timeRemainingOnAnimation < 0)
        {
            charAnims.SetTriggerFromString("StopClimb");

            MoveTo(ledgeHit.point);

            playerState = State.nothing;

            playerCollider.enabled = true;

            rb.useGravity = true;
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
                    StopDashing(false);
            }
            else if (playerState.Equals(State.dashing))
            {
                StopDashing(false);
            }
        }
    }

    private void Dash()
    {
        energy.SpendEnergy(DASH_ENERGY_COST);

        if (rb.useGravity)
            rb.useGravity = false;

        RaycastHit hit;

        if (Physics.SphereCast(frontFeetGroundCheck.position, 0.1f, transform.forward, out hit, DASH_DISTANCE_CHECK, ~playerLayer) || !energy.CheckEnergy(DASH_ENERGY_COST))
        {
            StopDashing(true);
        }
        else
        {
            if (energy.CheckEnergy(DASH_ENERGY_COST) && !playerState.Equals(State.dashing))
            {
                playerState = State.dashing;
                rb.AddForce(transform.forward * DASH_FORCE, ForceMode.Impulse);
                //constant force results in constant accelaration, zero force results constant velocity
            }
        }
        dashEffectsReference.SlowDown();
    }


    private void StopDashing(bool forceStop)
    {
        if (dashTimer <= 0f || forceStop)
        {
            ActivateRenderer(0);
            dashEffectsReference.SpeedUp();
            rb.velocity = new Vector3(0, 0, 0);
            playerState = State.nothing;
            dashCooldown = 1f;
            dashTimer = 0.2f;
        }
    }
    #endregion

    /*private void Gravity()
    {

        if (CheckGround()) //On ground gravity
        {

            if (inAir)
            {
                charAnims.SetTriggerFromString("Land");
                inAir = false;
            }
        }
        else
        {
            if(rb.velocity.y > 0f)
            {
                rb.AddForce(Vector3.down * GRAVITY_JUMP_APEX);
            }
            else
                rb.AddForce(Vector3.down * GRAVITY_VALUE);

            charAnims.SetAnimFloat("YSpeed", rb.velocity.y);

            if (!inAir)
                inAir = true;
        }
    }*/


    /*private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //Jump
        {
            if (CheckGround())
            {
                charAnims.SetTriggerFromString("Jump");
                inAir = true;
                rb.AddForce(rb.velocity.x, JUMP_HEIGHT, rb.velocity.z, ForceMode.Impulse);
            }
        }
    }*/

    private bool CheckGround(Transform groundcheck)
    {
        return Physics.CheckSphere(groundcheck.position, GROUND_CHECK_RADIUS, groundMask);
    }

    public void MoveTo(Vector3 position)
    {
        rb.velocity = Vector3.zero;
        rb.position = position;
    }

    public void ActivateRenderer(int index)
    {
        rend.sharedMaterial = materials[index]; //To switch shaders when using ability
    }

    public float GetVelocity()
    {
        return rb.velocity.magnitude;
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
