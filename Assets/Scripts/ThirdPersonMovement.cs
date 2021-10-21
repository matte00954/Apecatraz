using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Main camera")]
    [SerializeField] private Transform mainCameraTransform;

    [Header("Controller")]
    [SerializeField] private CharacterController controller;

    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    //Changes during runtime
    private float turnSmoothVelocity;
    private bool isTeleporting;
    private Vector3 velocity;

    //ground check
    private const float GroundCheckRadius = 0.15f; // comparing ground check game object to floor

    //Rotation
    private const float turnSmoothTime = 0.1f;

    //Teleport
    private const float teleportDistanceMultiplier = 0.15f; //per frame
    private const float teleportDistanceCheck = 0.5f;
    private const float teleportMarginMultiplier = 0.8f;

    //movement, these are constant
    private const float playerSpeed = 6f; //Do not change
    private const float jumpHeight = 4f; //Do not change

    //gravity
    private const float GravityValue = -9.81f; // do not change this -9.81f
    private const float GravityMultiplier = 1.5f; //multiplies gravity force

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined; //prevents mouse from leaving screen

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
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime); //adjust angle for smoothing

            transform.rotation = Quaternion.Euler(0f, angle, 0f); //adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; //adjust direction to camera rotation/direction

            ControllerMove(moveDirection * playerSpeed * Time.deltaTime);
        }
    }

    private void Teleport()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isTeleporting = true;

            Time.timeScale = 0.35f;

            RaycastHit hit;

            if (Physics.SphereCast(transform.position, 1f, transform.forward, out hit, teleportDistanceCheck))
            {
                ControllerMove(transform.forward * hit.distance * teleportMarginMultiplier);
            }
            else
                ControllerMove(transform.forward * teleportDistanceMultiplier);
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
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * GravityValue);
        }

        if (CheckGround() && velocity.y < 0)
        {
            velocity.y = -2f; //Default gravity force on the ground
        }
        else
            velocity.y += GravityValue * GravityMultiplier * Time.deltaTime; //gravity in the air

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

    //comment out this if climbing does not work
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Climb"))
        {
            Debug.Log("Climb trigger hit");
            Climb(other.gameObject.GetComponentInChildren<ClimbTransforms>());
        }
    }

    //comment out this if climbing does not work
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
                Debug.Log("Första siffran är: " + minDistance);
            }

            if (minDistance > Vector3.Distance(currentPosition, climbTransforms.GetClimbPositionInList(i).position))
            {
                minDistance = Vector3.Distance(currentPosition, climbTransforms.GetClimbPositionInList(i).position);
                closestPosition = climbTransforms.GetClimbPositionInList(i).position;
                Debug.Log(i + " siffran är: " + minDistance);
            }
        }
        velocity.y = 0;
        controller.enabled = false;
        transform.position = closestPosition;
        controller.enabled = true;
    }

}