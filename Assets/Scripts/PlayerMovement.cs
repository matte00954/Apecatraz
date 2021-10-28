using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [Header("Movement")]
    [SerializeField] private float playerSpeed = 5.0f;
    [SerializeField] private float jumpHeight = 1.0f;

    private Camera mainCamera;

    private Vector3 movement;
    private CharacterController controller;
    private Vector3 playerVelocity;

    private readonly float GravityMultiplier = 2.0f;
    private readonly float GravityValue = -9.81f; // dont change this -9.81f
    private readonly float GroundCheckRadius = 0.2f; // comparing ground check game object to floor

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        if (controller.isGrounded)
        {
            movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            movement = transform.TransformDirection(movement);
            movement *= playerSpeed;

            if (Input.GetKey(KeyCode.Space))
               movement.y += Mathf.Sqrt(jumpHeight * -3.0f * GravityValue);
        }

	movement.y += GravityMultiplier * GravityValue * Time.deltaTime;
	controller.Move(movement * Time.deltaTime);
    
    }
    private bool CheckGround()
    {
        //The alternative check if player is grounded by casting CheckSphere.
        return Physics.CheckSphere(groundCheck.position, GroundCheckRadius, groundMask);
    }
}
