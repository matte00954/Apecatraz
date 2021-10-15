using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform camera;

    [SerializeField] private float playerSpeed = 6f;
    [SerializeField] private float jumpHeight = 6f;


    [SerializeField] private float turnSmoothTime = 0.1f;

    private float turnSmoothVelocity;

    private readonly float GravityMultiplier = 2.0f;
    private readonly float GravityValue = -9.81f; // dont change this -9.81f
    private readonly float GroundCheckRadius = 0.2f; // comparing ground check game object to floor

    private void Start()
    {
        if(camera == null)
        {
            Debug.LogError("Camera not assigned to movement script, rotation will not work");
        }
        if (controller == null)
        {
            Debug.LogError("Controller not assigned to movement script");
        }
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y; //first find target angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime); //adjust angle for smoothing

            transform.rotation = Quaternion.Euler(0f, angle, 0f); //adjusted angle used here for rotation

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; //adjust direction to camera rotation/direction

            if (controller.isGrounded)
            {
                if (Input.GetKey(KeyCode.Space))
                    moveDirection.y += Mathf.Sqrt(jumpHeight * -3.0f * GravityValue);

                // movement.y = jumpHeight;
            }

            direction.y += GravityMultiplier * GravityValue * Time.deltaTime;

            controller.Move(moveDirection * playerSpeed * Time.deltaTime);
        }
    }
}
