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

        ControllerMove(velocity * Time.deltaTime);//gravity applied
    }

    private void ControllerMove(Vector3 movement) //THIS IS THE ONLY controller.Move that should exist
    {
        controller.Move(movement);
    }

    private bool CheckGround()
    {
        return Physics.CheckSphere(groundCheck.position, GroundCheckRadius, groundMask);
    }
}

/*
if (Input.GetKeyDown(KeyCode.LeftShift))
{
    RaycastHit hit;

    Debug.DrawRay(transform.position, transform.forward, Color.red, teleportDistance); //kan behövas en spherecast istället

    if (Physics.Raycast(transform.position, transform.forward, out hit, teleportDistance))
    {
        ControllerMove(transform.forward * hit.distance * teleportMarginMultiplier);
    }
    else
        ControllerMove(transform.forward * teleportDistance);
}
else if (Input.GetKeyDown(KeyCode.T))//WIP
{
    Time.timeScale = 0.1f;

    RaycastHit hit;

    Debug.DrawRay(transform.position, transform.forward, Color.blue, teleportDistance);

    if (Physics.Raycast(transform.position, transform.forward, out hit, teleportDistance * 1.5f)) //magic number
    {
        ControllerMove(transform.forward * hit.distance * teleportMarginMultiplier);
        RestoreTime();
    }
    else
    {
        ControllerMove(transform.forward * teleportDistance * 1.5f);
        RestoreTime();
    }*/
