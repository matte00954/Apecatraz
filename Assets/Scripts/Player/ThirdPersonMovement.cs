// Author: Mattias Larsson
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThirdPersonMovement : MonoBehaviour
{
    // dash
    private const float DashDistanceCheck = 0.25f;
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

    //Constants to make sure you get an error in code if you try to change these values

    [Header("Main camera")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("This one is on the camera")]
    [SerializeField] private DashEffects dashEffectsReference;

    [Header("Controller")]
    [SerializeField] private Rigidbody playerRigidbody;
    [Tooltip("WIP, does not work")]
    [SerializeField] private Transform gfxTransformForRotation; //might use this to flip gfx for slopes, currently not working 2022-01-06

    [Header("Ground check")]
    [Tooltip("Transform close to front feet in player prefab")]
    [SerializeField] private Transform frontFeetTransform;
    [Tooltip("Transform close to back feet in player prefab")]
    [SerializeField] private Transform backFeetTransform;

    [Header("Layer masks")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Climbable layer masks")]
    [SerializeField] private LayerMask ledgeMask;
    [SerializeField] private LayerMask groundMask;
    [Tooltip("Layers that stop dash")]
    [SerializeField] private LayerMask dashObstacles;

    [Header("Ledge")]
    [SerializeField] private CharAnims charAnims;
    [Tooltip("Just to get the transform")]
    [SerializeField] private GameObject ledgeDownCheck;
    [Tooltip("Just to get the transform")]
    [SerializeField] private GameObject ledgeUpCheck;
    [SerializeField] private AnimationClip climbAnimation;
    [Tooltip("In GFX inside player prefab")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    // ALL CLIMBABLE OBJECTS NEEDS A TRIGGER WITH CLIMB LAYER

    [Header("Steps")]
    [Tooltip("The maximum a player can set upwards in units when they hit a wall that's potentially a step")]
    [SerializeField] private float maxStepHeight = 0.1f;

    [Tooltip("How much to overshoot into the direction a potential step in units when testing. High values prevent player from walking up small steps but may cause problems")]
    [SerializeField] private float stepSearchOvershoot = 0.01f; 

    private List<ContactPoint> contactPoints = new List<ContactPoint>(); //Contact points are generated in OnCollision Methods and then cleared in fixedupdate
    private Vector3 lastVelocity;

    [Header("Energy")]
    [SerializeField] private Energy energy;

    [Header("Raycast transforms")]
    [Tooltip("A transform near head for raycasts")]
    [SerializeField] private Transform headRaycastOrigin;
    [Tooltip("A transform middle of body for raycasts")]
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

    [Header("Mechanics/cheats allowed in scene")]
    [Tooltip("Should be true unless in a testing scene")]
    [SerializeField] private bool dashAllowed = true;
    [Tooltip("Should be true unless in a testing scene")]
    [SerializeField] private bool ledgeGrabAllowed = true;
    [Tooltip("Should be true unless in a testing scene")]
    [SerializeField] private bool telekinesAllowed = true;
    [Tooltip("For going up smaller surfaces, can be CPU intensive")]
    [SerializeField] private bool stepUpAllowed = true; //Going up small surfaces
    [Tooltip("Allows teleporting between checkpoints for testing/debugging, with I, O and P keys")]
    [SerializeField] private bool godMode = false; // gives player a few cheats for instance the player can teleport to checkpoints with I, O and P

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

    private void Movement() //FixedUpdate
    {
        Vector3 direction = new Vector3(horizontal, 0f, vertical);

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

                    if (OnSlope())
                    {
                        //Attempts at rotating mesh on slopes
                        //Method 1, does not work
                        ////playerRigidbody.MoveRotation(Quaternion.Euler(angle, rb.transform.rotation.y, 0f));

                        //Method 2, does not work
                        ////Transform transform = OnSlopeVector(); 
                        ////gfxTransformForRotation.rotation = Quaternion.Euler(transform.rotation.eulerAngles);
                        ///

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
                        if(stepUpAllowed)
                            StepUp();

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

        contactPoints.Clear(); //Only need the latest ones, this is for small steps

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

        // Jump input in update, otherwise it feels delayed
        if (jump)
        {
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, JumpHeight, playerRigidbody.velocity.z);

            charAnims.SetTriggerFromString("Jump");

            jump = false;
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
                        // If climbing looks weird in game, move the climb triggers in the scene to a better position
                    }
                }
            }
        }
    }

    private void StepUp()
    {

        Vector3 velocity = playerRigidbody.velocity;

        //Filter through the ContactPoints to see if we're grounded and to see if we can step up
        ContactPoint groundContactPoint = default(ContactPoint);
        bool grounded = FindGround(out groundContactPoint, contactPoints);

        Vector3 stepUpOffset = default(Vector3);
        bool stepUp = false;

        if (grounded)
            stepUp = FindStep(out stepUpOffset, contactPoints, groundContactPoint, velocity);

        if (stepUp)
        {
            playerRigidbody.position += stepUpOffset;
            playerRigidbody.velocity = lastVelocity;
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

                MoveTo(ledgeHit.point + new Vector3(0, 0.4f, 0)); // Adds marigin to make sure player gets on top of object, without this you might get stuck

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
                if (dashTimer <= 0)
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

        if(stepUpAllowed)
            StepUp();

        RaycastHit hit; //Not really needed

        if (Physics.SphereCast(headRaycastOrigin.position, 0.1f, transform.forward, out hit, DashDistanceCheck, dashObstacles) || !energy.CheckEnergy(DashEnergyCost))
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
        dashEffectsReference.SpeedUp(); //This is on the camera (not sure why)
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

        //if (horizontal != 0 || vertical != 0) //only collects data if any movement input is detected

        contactPoints.AddRange(collision.contacts);
    }
    private void OnCollisionStay(Collision collision)
    {
        contactPoints.AddRange(collision.contacts);
    }

    #region HandlingSteps

    // I followed this tutorial, this article was exactly what i wanted and needed so it is almost exactly the same
    //https://cobertos.com/blog/post/how-to-climb-stairs-unity3d/

    private bool FindGround(out ContactPoint groundContactPoint, List<ContactPoint> allContactPoints)
    {
        groundContactPoint = default(ContactPoint);
        bool found = false;

        foreach (ContactPoint cp in allContactPoints)
        {
            if (cp.normal.y > 0.0001f && (found == false || cp.normal.y > groundContactPoint.normal.y))
            {
                groundContactPoint = cp;
                found = true;
            }
        }
        return found;
    }

    private bool FindStep(out Vector3 stepUpOffset, List<ContactPoint> allContactPoints, ContactPoint groundContactPoint, Vector3 currentVelocity)
    {
        stepUpOffset = default(Vector3);

        Vector2 velocityXZ = new Vector2(currentVelocity.x, currentVelocity.y);

        if(velocityXZ.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        foreach (ContactPoint cp in allContactPoints)
        {
            bool test = ResolveStepUp(out stepUpOffset, cp, groundContactPoint);

            if (test)
                return test;
        }
        return false;
    }

    private bool ResolveStepUp(out Vector3 stepUpOffset, ContactPoint stepTestContactPoint, ContactPoint groundContactPoint)
    {
        stepUpOffset = default(Vector3);
        Collider stepCollider = stepTestContactPoint.otherCollider;

        //( 1 ) Check if the contact point normal matches that of a step (y close to 0)
        if (Mathf.Abs(stepTestContactPoint.normal.y) >= 0.01f)
        {
            return false;
        }

        //( 2 ) Make sure the contact point is low enough to be a step
        if (!(stepTestContactPoint.point.y - groundContactPoint.point.y < maxStepHeight))
        {
            return false;
        }

        //( 3 ) Check to see if there's actually a place to step in front of us
        //Fires one Raycast
        RaycastHit hitInfo;
        float stepHeight = groundContactPoint.point.y + maxStepHeight + 0.0001f; 
        Vector3 stepTestInverseDirection = new Vector3(-stepTestContactPoint.normal.x, 0, -stepTestContactPoint.normal.z).normalized;
        Vector3 origin = new Vector3(stepTestContactPoint.point.x, stepHeight, stepTestContactPoint.point.z) + (stepTestInverseDirection * stepSearchOvershoot);
        Vector3 direction = Vector3.down;
        if (!stepCollider.Raycast(new Ray(origin, direction), out hitInfo, maxStepHeight))
        {
            return false;
        }

        //We have enough info to calculate the points
        Vector3 stepUpPoint = new Vector3(stepTestContactPoint.point.x, hitInfo.point.y + 0.0001f, stepTestContactPoint.point.z) + (stepTestInverseDirection * stepSearchOvershoot);
        Vector3 stepUpPointOffset = stepUpPoint - new Vector3(stepTestContactPoint.point.x, groundContactPoint.point.y, stepTestContactPoint.point.z);

        //We passed all the checks! Calculate and return the point!
        stepUpOffset = stepUpPointOffset;
        return true;
    }
    #endregion
}