// Author: Mattias Larsson
using UnityEngine;
using UnityEngine.Events;

public class ThirdPersonMovement : MonoBehaviour
{
    // dash
    private const float DashDistanceCheck = 1f;
    private const float DashForce = 45f;

    // movement
    private const float MaxPlayerSpeedRun = 8.5f; 
    private const float MaxPlayerSpeedWalk = 4f;
    private const float JumpHeight = 22f; 

    // dash
    private const float DashEnergyCost = 5f;

    // gravity
    private const float GravityValue = 1.5f;
    private const float GravityJumpApex = 5f;
    private const float LedgeCheckRayLengthMultiplier = 1.5f;

    // rotation
    private const float TurnSmoothTime = 0.02f;
    private const float TurnSmoothTimeInAir = 0.25f;

    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DashEffects dashEffectsReference;

    [Header("Controller")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Transform gfxTransformForRotation; //might use this to flip gfx, currently not working

    [Header("Ground check")]
    [SerializeField] private Transform frontFeetTransform;
    [SerializeField] private Transform backFeetTransform;

    [Header("Layer masks")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask ledgeMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask dashObstacles;

    [Header("Ledge")]
    [SerializeField] private CharAnims charAnims;
    [SerializeField] private GameObject ledgeDownCheck;
    [SerializeField] private GameObject ledgeUpCheck;
    [SerializeField] private AnimationClip climbAnimation;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    ////[SerializeField] private Collider playerCollider;

    [Header("Energy")]
    [SerializeField] private Energy energy;

    [Header("Raycast transforms")]
    [SerializeField] private Transform headRaycastOrigin;
    [SerializeField] private Transform midRaycast;

    [Header("Ability Shaders")]
    [SerializeField] private Material[] materials;
    private Renderer rend;

    [Header("Events")]
    [SerializeField] private UnityEvent onRespawn;

    [Header("Audio")]
    [SerializeField] private AudioClip footSteps;
    [SerializeField] private AudioClip glassImpact;
    [SerializeField] private AudioClip genericImpact;

    [Header("ONLY FOR PROTOTYPES")]
    [SerializeField] private bool dashAllowed = true;
    [SerializeField] private bool ledgeGrabAllowed = true;
    [SerializeField] private bool telekinesAllowed = true;
    [SerializeField] private bool godMode = false; // gives player a few cheats for instance the player can teleport to checkpoints with I, O and P
    //[SerializeField] private bool slowmotionAllowed = false;

    [Header("Game handler")]
    [SerializeField] private GameManager gameManager;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float respawnTimer = 3f;

    // Changes during runtime
    private RaycastHit ledgeHit;
    private Vector3 slopeMoveDirection;
    private Vector3 slopeHitNormal;

    private bool isMoving = false;
    private bool landAnimationReady;
    private bool backFeetOnGround;
    private bool frontFeetOnGround;
    private bool jump;
    private bool resetVelocity;
    private bool walk;

    private float horizontal;
    private float vertical;
    private float turnSmoothVelocity;
    private float dashCooldown;
    private float dashTimer;
    private float timeRemainingOnAnimation;

    // ALL CLIMBABLE OBJECTS NEEDS A TRIGGER WITH CLIMB LAYER

    private State playerState;
    public enum State { dashing, telekinesis, disabled, nothing, climbing, hiding }
    public State PlayerState { get => playerState; set => playerState = value; }
    [HideInInspector] public bool IsTelekinesisActive { get; set; }
    public float Velocity { get => playerRigidbody.velocity.magnitude; }
    public bool IsGodMode { get => godMode; }
    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public bool IsFrontFeetGrounded { get => frontFeetOnGround; }
    public bool IsBackFeetGrounded { get => backFeetOnGround; }

    public void MoveTo(Vector3 position)
    {
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.position = position;
    }

    public void ToggleRigidbodyCollisions(bool toggle)
    {
        playerRigidbody.detectCollisions = toggle;
        playerRigidbody.useGravity = toggle;
    }

    public void ActivateRenderer(int index) => rend.sharedMaterial = materials[index]; // To switch shaders when using ability

    private void Start()
    {
        resetVelocity = true;

        dashTimer = 0.2f;

        if (!dashAllowed)
            Debug.Log("Dash not allowed!");
        if (!ledgeGrabAllowed)
            Debug.Log("Ledge climbing not allowed!");
        if (!telekinesAllowed)
            Debug.Log("Telekinesis not allowed!");
        if (godMode)
            Debug.Log("God mode on!");
        /*if (slowmotionAllowed)
            Debug.Log("Slow motion allowed, SHOULD ONLY BE ALLOWED IN PROTOTYPE!!!");*/

        IsTelekinesisActive = telekinesAllowed;
        playerState = State.nothing;
        Cursor.lockState = CursorLockMode.Locked; // Prevents mouse from leaving screen

        // Added by Emil
        rend = GetComponentInChildren<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[0];

        if (mainCamera.transform == null)
        {
            Debug.LogError("Camera not assigned to movement script, rotation will not work");
        }
    }

    private void Update()
    {
        if (!InGameMenuManager.GameIsPaused && !DialogueManager.IsPausedWhileReading)
        {
            backFeetOnGround = Physics.Raycast(backFeetTransform.position, Vector3.down, 0.4f, groundMask);
            frontFeetOnGround = Physics.Raycast(frontFeetTransform.position, Vector3.down, 0.4f, groundMask);

            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            // Makes sure the player cannot execute superjump while climbing
            if (!playerState.Equals(State.climbing) && Input.GetButtonDown("Jump"))
            {
                if (Physics.Raycast(backFeetTransform.position, Vector3.down, 0.5f, groundMask))
                {
                    jump = true;
                }

                if (Physics.Raycast(frontFeetTransform.position, Vector3.down, 0.5f, groundMask))
                {
                    jump = true;
                }
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                walk = true;
            }
            else if (walk)
            {
                walk = false;
            }

            /*
            // To unpause game
            if (Time.timeScale != 1 && !slowmotionAllowed)
            {
                Time.timeScale = 1;
            }

            if (Input.GetKeyDown(KeyCode.Y) && godMode)
            {
                slowmotionAllowed = !slowmotionAllowed;
            }

            // For Joche's prototype
            if (slowmotionAllowed)
            {
                ////if (PlayerState.Equals(State.dashing) && Time.timeScale != 0.2f)
                ////{
                ////    Time.timeScale = 0.2f;
                ////}
                ////else if (!PlayerState.Equals(State.dashing) && Time.timeScale != 1f)
                ////{
                ////    Time.timeScale = 1f;
                ////}

                // Equivalent solution (value = condition ? true : false)
                Time.timeScale = PlayerState.Equals(State.dashing) && Time.timeScale != 0.2f ? 0.2f 
                    : (!PlayerState.Equals(State.dashing) && Time.timeScale != 1f ? 1f : Time.timeScale);
            }*/

        }

        if (InGameMenuManager.GameIsPaused && Cursor.lockState.Equals(CursorLockMode.Locked))
            Cursor.lockState = CursorLockMode.None;
        else if (!InGameMenuManager.GameIsPaused && Cursor.lockState.Equals(CursorLockMode.None))
            Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if (!InGameMenuManager.GameIsPaused)
        {
            StateCheck();
        }
    }

    private void LateUpdate()
    {
        charAnims.SetAnimFloat("runY", playerRigidbody.velocity.magnitude); // Added by Joche
    }


    private void StateCheck()
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
            case State.disabled: // disabled = captured
                respawnTimer -= Time.deltaTime;
                Death();
                RespawnPlayer();
                break;
            case State.climbing:
                LedgeClimb();
                break;
            case State.nothing: // nothing = default
                Movement();
                LedgeCheck();
                DashCheck();
                break;
            case State.hiding: // hiding in trash can
                break;
            default: //this one should never happen
                Debug.LogError("Player state is null");
                break;
        }

        if (!playerState.Equals(State.dashing))
            if (dashCooldown >= 0f)
                dashCooldown -= Time.fixedDeltaTime;

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("IN AIR = " + (!frontFeetOnGround && !backFeetOnGround));
        }
    }

    private void Death()
    {
        //// throw new NotImplementedException();
        charAnims.SetAnimBool("Deth", true);
    }

    private void RespawnPlayer()
    {
        if (respawnTimer <= 0.0f)
        {
            charAnims.SetAnimBool("Deth", false);
            gameManager.RespawnAtLatestCheckpoint();
            playerState = State.nothing;
            respawnTimer = 3f;
            onRespawn.Invoke();
            Debug.Log("Player died");
        }
    }

    private void Movement()
    {
        Vector3 direction = new Vector3(horizontal, 0f, vertical);

        /*if (rb.drag != defaultDrag && frontFeetOnGround && backFeetOnGround)
        {
            rb.drag = defaultDrag;
        }*/

        if (horizontal == 0 && vertical == 0)
        {
        }

        if (direction.magnitude >= 0.01f)
        {
            float targetAngle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + mainCamera.transform.eulerAngles.y; // First find target angle
            float angle;

            ////if (!backFeetOnGround && !frontFeetOnGround)
            ////{
            ////    angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTimeInAir); // Adjust angle for smoothing in air
            ////}
            ////else
            ////{
            ////    angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); // Adjust angle for smoothing on ground
            ////}
            
            // Equivalent solution (variable = condition ? true : false)
            angle = !backFeetOnGround && !frontFeetOnGround ?
                Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTimeInAir) // Adjust angle for smoothing in air
                : Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); // Adjust angle for smoothing on ground

            playerRigidbody.MoveRotation(Quaternion.Euler(0f, angle, 0f));
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // Adjust direction to camera rotation/direction
            slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHitNormal);

            if (horizontal != 0 || vertical != 0)
            {
                if (backFeetOnGround || frontFeetOnGround)
                {
                    ////float difference = Mathf.Abs(rb.velocity.magnitude - MaxPlayerSpeedRun);

                    if (OnSlope())
                    {
                        //Method 1, does not work
                        ////playerRigidbody.MoveRotation(Quaternion.Euler(angle, rb.transform.rotation.y, 0f));

                        //Method 2, does not work
                        ////Transform transform = OnSlopeVector(); 
                        ////gfxTransformForRotation.rotation = Quaternion.Euler(transform.rotation.eulerAngles);
                        
                        if (walk) //walking on slopes
                        {
                            if (playerRigidbody.velocity.magnitude < MaxPlayerSpeedWalk)
                                playerRigidbody.AddForce(slopeMoveDirection.normalized, ForceMode.Impulse);
                        }
                        else if (playerRigidbody.velocity.magnitude < MaxPlayerSpeedRun)
                        {
                            playerRigidbody.AddForce(slopeMoveDirection.normalized, ForceMode.Impulse);
                        }
                    }
                    else //Not on slope, regular flat movement
                    {
                        if (walk)
                        {
                            if (playerRigidbody.velocity.magnitude < MaxPlayerSpeedWalk)
                            {
                                playerRigidbody.AddForce(moveDirection, ForceMode.Impulse);
                            }
                        }
                        else if (playerRigidbody.velocity.magnitude < MaxPlayerSpeedRun)
                        {
                            playerRigidbody.AddForce(moveDirection * 1.15f, ForceMode.Impulse);
                        }
                    }
                }
                else
                {
                    Vector3 velocityWithoutY = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z); // Remove Y velocity from calc

                    if (velocityWithoutY.magnitude < MaxPlayerSpeedRun)
                    {
                        playerRigidbody.AddForce(moveDirection, ForceMode.Impulse); // In air
                    }
                }
            }
        }

        if (frontFeetOnGround || backFeetOnGround)
            charAnims.CheckStopRunning();

        // In air
        if (!frontFeetOnGround && !backFeetOnGround)
        {
            if (playerRigidbody.velocity.y < 0f)
                playerRigidbody.AddForce(Physics.gravity * GravityJumpApex);
            else
                playerRigidbody.AddForce(Physics.gravity * GravityValue);

            if (!landAnimationReady)
                landAnimationReady = true;

            charAnims.SetAnimFloat("YSpeed", playerRigidbody.velocity.y);
        }

        if ((frontFeetOnGround && landAnimationReady)
            || (backFeetOnGround && landAnimationReady))
        {
            charAnims.SetTriggerFromString("Land");
            landAnimationReady = false;
        }

        if (jump)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, JumpHeight, playerRigidbody.velocity.z);

            charAnims.SetTriggerFromString("Jump");

            jump = false; // Jump input set in update, otherwise too delayed
        }
    }

    private bool OnSlope()
    {
        RaycastHit slopeHit;

        if (Physics.Raycast(midRaycast.position, Vector3.down, out slopeHit, 0.5f, groundMask))
        {
            if (slopeHit.normal != Vector3.up)
            {
                slopeHitNormal = slopeHit.normal;
                return slopeHit.normal != Vector3.up;
            }
            else
                return false;
        }
        else
            return false;
    }

    private Transform OnSlopeVector() // does not work as intended
    {
        RaycastHit slopeHit;

        Physics.Raycast(midRaycast.position, Vector3.down, out slopeHit, 0.5f, groundMask);

        return slopeHit.transform;
    }

    #region Ledgeclimb
    private void LedgeCheck()
    {
        if (ledgeGrabAllowed)
        {
            RaycastHit upperHit;

            // If player is above obstacle, do not climb
            if (Physics.Raycast(ledgeUpCheck.transform.position, Vector3.up * LedgeCheckRayLengthMultiplier, out upperHit, LedgeCheckRayLengthMultiplier))
            {
                return;
            }
            else
            {
                RaycastHit downHit; // Ray from ledge check game object

                // Checks if target surface has "climb" layer
                if (Physics.Raycast(ledgeDownCheck.transform.position, Vector3.down * LedgeCheckRayLengthMultiplier, out downHit, LedgeCheckRayLengthMultiplier, ledgeMask))
                {
                    RaycastHit forwardHit;

                    // Checks distance from object so animation starts at correct the distance
                    if (Physics.Raycast(frontFeetTransform.transform.position, transform.forward * LedgeCheckRayLengthMultiplier, out forwardHit, LedgeCheckRayLengthMultiplier)) 
                    {
                        playerRigidbody.useGravity = false; // Otherwise player might float under object

                        playerRigidbody.velocity = Vector3.zero;

                        // Adjusts player position before animation
                        // y - y = height of object - height of player
                        MoveTo(new Vector3(forwardHit.point.x, downHit.point.y - skinnedMeshRenderer.bounds.extents.y, forwardHit.point.z));

                        playerState = State.climbing;
                        charAnims.SetTriggerFromString("LedgeGrab");
                        ledgeHit = downHit; // Target position of climb
                        timeRemainingOnAnimation = climbAnimation.length;

                        // Method LedgeClimb() starts in update if playerstate is climbing
                    }
                }
            }
        }
    }

    private void LedgeClimb()
    {
        // To make sure this only happens once, since this is set false in ledgecheck
        if (!playerRigidbody.useGravity)
        {
            timeRemainingOnAnimation -= Time.fixedDeltaTime;

            if (timeRemainingOnAnimation < 0)
            {
                charAnims.SetTriggerFromString("StopClimb");

                MoveTo(ledgeHit.point + new Vector3(0, 0.4f, 0)); // Adds marigin to make sure to not climb inside object instead of on top

                playerState = State.nothing;

                playerRigidbody.useGravity = true;
            }
        }
    }
    #endregion

    #region Dash
    private void DashCheck()
    {
        if (dashAllowed)
        {
            if (Input.GetButton("Fire1") && dashCooldown <= 0f)
            {
                if (energy.CheckEnergy(DashEnergyCost))
                {
                    if (energy.IsRegenerating)
                        energy.IsRegenerating = false;

                    Dash();
                }
                else
                {
                    StopDashing();
                }
            }
            else
            { 
                if(dashTimer <= 0)
                    StopDashing();
            }
        }
    }

    private void Dash()
    {
        ActivateRenderer(1); // adds effect during dash

        energy.SpendEnergy(DashEnergyCost);

        if (resetVelocity)
        {
            playerRigidbody.velocity = Vector3.zero;
            resetVelocity = false;
        }

        dashEffectsReference.SlowDown();

        RaycastHit hit;

        if (Physics.SphereCast(headRaycastOrigin.position, 0.15f, transform.forward, out hit, DashDistanceCheck, dashObstacles) || !energy.CheckEnergy(DashEnergyCost))
        {
            StopDashing();
        }
        else if (energy.CheckEnergy(DashEnergyCost) && !playerState.Equals(State.dashing))
        {
            playerState = State.dashing;
            playerRigidbody.velocity = transform.forward * DashForce;
            // Constant force results in constant accelaration, zero force results constant velocity
        }

    }

    private void StopDashing()
    {
        ActivateRenderer(0); //Removes dash effect on player
        dashEffectsReference.SpeedUp();
        resetVelocity = true; 
        playerState = State.nothing; 
        dashCooldown = 1f; // cooldown to disable spamming dash
        dashTimer = 0.2f; // timer reset
        energy.IsRegenerating = true;
    }
    #endregion

    /*private void OnDrawGizmos()
    {
        ////backFeetOnGround = Physics.CheckSphere(backFeetGroundCheck.position, GROUND_CHECK_RADIUS, ~playerLayer);
        ////frontFeetOnGround = Physics.CheckSphere(frontFeetGroundCheck.position, GROUND_CHECK_RADIUS, ~playerLayer);
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Projectile")))
            if (other.gameObject.GetComponent<ProjectileNet>().IsActive)
                playerState = State.disabled;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("GlassImpact"))
            audioSource.PlayOneShot(glassImpact);

        if (collision.gameObject.CompareTag("GenericImpact"))
            audioSource.PlayOneShot(genericImpact);
    }
}