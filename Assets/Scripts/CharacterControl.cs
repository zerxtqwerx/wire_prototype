using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    CharacterController characterController;
    public Transform orientation;

    private Vector3 playerVelocity;

    private bool groundedPlayer;
    public float playerSpeed = 2.0f;

    public float jumpForce;

    [Header("Keybinds")]
    public KeyCode jumpKey;


    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Vector3 velocity = Vector3.zero;
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer)
            velocity = Vector3.zero;
        MyInput();
        MovePlayer();
        Fall();
        characterController.Move((Vector3)(velocity * Time.deltaTime));
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && groundedPlayer)
        {
            Jump();
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        moveDirection.Normalize();

        moveDirection *= playerSpeed;

        velocity.Set(moveDirection.x, velocity.y, moveDirection.z);
    }

    private void Fall()
    {
        velocity += Physics.gravity * Time.deltaTime;
    }

    private void Jump()
    {
        velocity += Vector3.up * jumpForce;
    }
}
