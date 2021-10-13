using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    private CharacterController controller;
    private Vector3 playerVelocity;

    private bool groundedPlayer;

    private float playerSpeed = 2.0f; // dont change this
    private float jumpHeight = 1.0f; // dont change this
    private float gravityValue = -9.81f; // dont change this
    private float groundCheckRadius = 0.2f; // dont change this

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
        controller.Move(move * Time.deltaTime * playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);  
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
