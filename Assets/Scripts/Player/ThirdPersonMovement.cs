// Author: Mattias Larsson
using System;
using UnityEngine;
using UnityEngine.Events;

public class ThirdPersonMovement : MonoBehaviour
{
    // teleport
    private const float DashDistanceCheck = 1f;
    private const float DashForce = 45f;

    // movement
    private const float MaxPlayerSpeedRun = 8f; // Do not change
    private const float MaxPlayerSpeedWalk = 5f; // Do not change
    private const float PlayerSpeedDividerInAir = 5f; // Do not change
    private const float JumpHeight = 22f; // Do not change

    // dash
    private const float DashEnergyCost = 5f;

    // gravity
    private const float GravityValue = 1.5f;
    private const float GravityJumpApex = 5f;
    private const float LedgeCheckRayLengthMultiplier = 1.5f;

    // ground check
    private const float GroundCheckRadius = 0.3f; // comparing ground check game object to floor

    // rotation
    private const float TurnSmoothTime = 0.05f;
    private const float TurnSmoothTimeInAir = 0.25f;

    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DashEffects dashEffectsReference;

    [Header("Controller")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PhysicMaterial pm;

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
    [SerializeField] private bool godMode = false; // only effects slowmotion atm
    [SerializeField] private bool slowmotionAllowed = false;


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
    ////private float defaultDrag;
    ////private float deafaltDynamicFriction;
    // ALL CLIMBABLE OBJECTS NEEDS A TRIGGER WITH CLIMB LAYER

    // Player States
    private State playerState;
    public enum State { dashing, telekinesis, disabled, nothing, climbing, hiding }

    public State PlayerState { get => playerState; set => playerState = value; }
    [HideInInspector] public bool IsTelekinesisActive { get; set; }

    private void Start()
    {
        /*deafaltDynamicFriction = pm.dynamicFriction;
        defaultDrag = rb.drag;*/
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
        if (slowmotionAllowed)
            Debug.Log("Slow motion allowed, SHOULD ONLY BE ALLOWED IN PROTOTYPE!!!");

        if (!telekinesAllowed)
        {
            IsTelekinesisActive = false;
        }
        else
            IsTelekinesisActive = true;

        playerState = State.nothing;

        Cursor.lockState = CursorLockMode.Locked; // prevents mouse from leaving screen

        // Emils grej
        rend = GetComponentInChildren<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = materials[0];

        if (mainCamera.transform == null)
        {
            Debug.LogError("Camera not assigned to movement script, rotation will not work");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!InGameMenuManager.gameIsPaused)
        {
            backFeetOnGround = Physics.Raycast(backFeetTransform.position, Vector3.down, 0.2f, groundMask);
            frontFeetOnGround = Physics.Raycast(frontFeetTransform.position, Vector3.down, 0.2f, groundMask);

            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            // ser till att man inte kan få ett superhopp samtidigt som man klättrar
            if (!playerState.Equals(State.climbing))
            {
                jump = Physics.Raycast(backFeetTransform.position, Vector3.down, 0.5f, groundMask) && Input.GetButtonDown("Jump");
                ////RaycastHit raycastHit;
                ////jump = Physics.SphereCast(backFeetTransform.position, 1f, Vector3.down, out raycastHit, groundMask) && Input.GetButtonDown("Jump");
            }

            walk = Input.GetKey(KeyCode.LeftControl);

            // to unpause game
            if (Time.timeScale != 1 && !slowmotionAllowed)
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

    private void StateCheck() // this is in update
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
            case State.disabled: // disabled = captured/rekt
                // spela death anim
                respawnTimer -= Time.deltaTime;
                Death();
                RespawnPlayer();
                // reset spel
                break;
            case State.climbing:
                LedgeClimb();
                break;
            case State.nothing:
                Movement();
                LedgeCheck();
                DashCheck();
                break;
            case State.hiding:

                break;
            default:
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
        //throw new NotImplementedException();
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

        if (direction.magnitude >= 0.1f)
        {

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y; // first find target angle
            float angle;

            if (!backFeetOnGround && !frontFeetOnGround)
            {
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTimeInAir); // adjust angle for smoothing in air
            }
            else
            {
                angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TurnSmoothTime); // adjust angle for smoothing on ground
            }

            rb.MoveRotation(Quaternion.Euler(0f, angle, 0f));

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // adjust direction to camera rotation/direction

            slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHitNormal);

            if (horizontal != 0 || vertical != 0)
            {
                if (backFeetOnGround || frontFeetOnGround)
                {
                    float difference = Mathf.Abs(rb.velocity.magnitude - MaxPlayerSpeedRun);

                    if (OnSlope())
                    {
                        ////rb.MoveRotation(Quaternion.Euler(angle, rb.transform.rotation.y, 0f));
                        if (rb.velocity.magnitude < MaxPlayerSpeedRun)
                            rb.AddForce(slopeMoveDirection.normalized * difference, ForceMode.Impulse);
                    }
                    else
                    {
                        if (walk)
                        {
                            if (rb.velocity.magnitude < MaxPlayerSpeedWalk)
                                rb.AddForce(moveDirection * difference, ForceMode.Impulse);
                        }
                        else if (rb.velocity.magnitude < MaxPlayerSpeedRun)
                        {
                            if (rb.velocity.magnitude < MaxPlayerSpeedRun)
                                rb.AddForce(moveDirection * difference, ForceMode.Impulse);
                        }
                    }
                }
                else
                {
                    Vector3 velocityWithoutY = new Vector3(rb.velocity.x, 0f, rb.velocity.z); //remove Y velocity from calc
                    if (velocityWithoutY.magnitude < MaxPlayerSpeedRun)
                        rb.AddForce(moveDirection, ForceMode.Impulse); // In air
                }
            }
        }

        if (frontFeetOnGround || backFeetOnGround)
        {
            charAnims.CheckStopRunning();
        }

        // gravity
        if (!frontFeetOnGround && !backFeetOnGround)  // In air
        {
            if (rb.velocity.y < 0f)
            {
                rb.AddForce(Physics.gravity * GravityJumpApex);
            }
            else
                rb.AddForce(Physics.gravity * GravityValue);

            if (!landAnimationReady)
                landAnimationReady = true;

            charAnims.SetAnimFloat("YSpeed", rb.velocity.y);
        }

        if (frontFeetOnGround && landAnimationReady
            || backFeetOnGround && landAnimationReady)
        {
            charAnims.SetTriggerFromString("Land");
            landAnimationReady = false;
        }

        if (jump)
        {
            rb.AddForce(new Vector3(0, JumpHeight, 0), ForceMode.VelocityChange);

            charAnims.SetTriggerFromString("Jump");

            jump = false; // jump input set in update, otherwise too delayed
        }
    }

    private void LateUpdate()
    {
        charAnims.SetAnimFloat("runY", rb.velocity.magnitude); // Joches grej
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

    #region Ledgeclimb
    private void LedgeCheck()
    {
        if (ledgeGrabAllowed)
        {
            RaycastHit upHit;

            if (Physics.Raycast(ledgeUpCheck.transform.position, Vector3.up * LedgeCheckRayLengthMultiplier,
                out upHit, LedgeCheckRayLengthMultiplier)) //if player is above obstacle, do not climb
            {
                return;
            }
            else //nothing in the way
            {
                RaycastHit downHit; //ray from ledge check game object

                if (Physics.Raycast(ledgeDownCheck.transform.position, Vector3.down * LedgeCheckRayLengthMultiplier,
                    out downHit, LedgeCheckRayLengthMultiplier, ledgeMask)) //checks if target surface has "climb" layer
                {
                    RaycastHit forwardHit;

                    if (Physics.Raycast(frontFeetTransform.transform.position, transform.forward * LedgeCheckRayLengthMultiplier,
                        out forwardHit, LedgeCheckRayLengthMultiplier)) //checks distance from object so animation starts at correct the distance
                    {
                        rb.useGravity = false; //otherwise player might float under object

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
        if (!rb.useGravity) //to make sure this only happens once, since this is set false in ledgecheck
        {
            timeRemainingOnAnimation -= Time.fixedDeltaTime;

            if (timeRemainingOnAnimation < 0)
            {
                charAnims.SetTriggerFromString("StopClimb");

                MoveTo(ledgeHit.point + new Vector3(0, 0.4f, 0)); //adds marigin to make sure to not climb inside object instead of on top

                playerState = State.nothing;

                rb.useGravity = true;
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
                    ActivateRenderer(1);
                    energy.ActivateEnergyRegen(false);
                    Dash();
                }
                else
                {
                    StopDashing(false);
                }
            }
            else if (playerState.Equals(State.dashing))
            {
                StopDashing(false);
            }
        }
    }

    private void Dash()
    {
        energy.SpendEnergy(DashEnergyCost);

        if (resetVelocity)
        {
            rb.velocity = Vector3.zero;
            resetVelocity = false;
        }

        if (pm.dynamicFriction != 0)
            pm.dynamicFriction = 0f;

        if (rb.useGravity)
            rb.useGravity = false;

        RaycastHit spherecast;

        if (Physics.SphereCast(headRaycastOrigin.position, DashDistanceCheck, Vector3.zero, out spherecast, dashObstacles) || !energy.CheckEnergy(DashEnergyCost))
        {
            StopDashing(true);
        }
        else
        {
            if (energy.CheckEnergy(DashEnergyCost) && !playerState.Equals(State.dashing))
            {
                playerState = State.dashing;
                if (rb.velocity != transform.forward * DashForce)
                    rb.velocity = transform.forward * DashForce;
                //rb.AddForce(transform.forward * DASH_FORCE, ForceMode.Impulse);
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
            rb.useGravity = true;
            resetVelocity = true;
            //pm.dynamicFriction = deafaltDynamicFriction;
            //rb.drag = defaultDrag;
            playerState = State.nothing;
            dashCooldown = 1f;
            dashTimer = 0.2f;
            energy.ActivateEnergyRegen(true);
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        ////backFeetOnGround = Physics.CheckSphere(backFeetGroundCheck.position, GROUND_CHECK_RADIUS, ~playerLayer);
        ////frontFeetOnGround = Physics.CheckSphere(frontFeetGroundCheck.position, GROUND_CHECK_RADIUS, ~playerLayer);
    }

    public void MoveTo(Vector3 position)
    {
        rb.velocity = Vector3.zero;
        rb.position = position;
    }

    public void ToggleRigidbodyCollisions(bool toggle)
    {
        rb.detectCollisions = toggle;
        rb.useGravity = toggle;
    }


    public void ActivateRenderer(int index)
    {
        rend.sharedMaterial = materials[index]; // To switch shaders when using ability
    }

    public float GetVelocity()
    {
        return rb.velocity.magnitude;
    }

    public bool GetGodMode()
    {
        return godMode;
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Projectile")))
            if (other.gameObject.GetComponent<ProjectileNet>().IsActive())
                playerState = State.disabled;
    }
    public bool GetFrontFeetGrounded()
    {
        return frontFeetOnGround;
    }
    public bool GetBackFeetGrounded()
    {
        return backFeetOnGround;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("GlassImpact"))
        {
            audioSource.PlayOneShot(glassImpact);
        }

        if (collision.gameObject.CompareTag("GenericImpact"))
        {
            audioSource.PlayOneShot(genericImpact);
        }
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
