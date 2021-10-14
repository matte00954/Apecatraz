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

    private CharacterController controller;
    private Vector3 playerVelocity;

    private bool groundedPlayer;

    private static float gravityValue = -9.81f; // dont change this -9.81f
    private static float groundCheckRadius = 0.2f; // comparing ground check game object to floor

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        Movement();
        Jump();
    }

    private void Movement()
    {
        groundedPlayer = controller.isGrounded;
        
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        
        playerVelocity.y += gravityValue * Time.deltaTime;
        move +=playerVelocity;
        controller.Move(move * Time.deltaTime * playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        //controller.Move(playerVelocity * Time.deltaTime);
        //transform.Rotate(Input.GetAxis("Mouse X"),0,0);  
        }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && CheckGround()) // Changes the height position of the player..
                {
                    playerVelocity.y += Mathf.Sqrt(jumpHeight * -5.0f * gravityValue);
                    // We might need gravity changes after jump
                }       

    }

    private bool CheckGround()
    {
        //The alternative check if player is grounded by casting CheckSphere.
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

    }
}
