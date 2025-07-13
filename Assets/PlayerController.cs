using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{


    public float gravityForce = 25f;
    [Range(1f, 10f)]
    public float movementSpeed = 5f;
    public float jumpForce = 8f;

    private float verticalVelocity;
    private bool isJumping = false;

    public Vector2 movementInput;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        float movementX = movementInput.x * movementSpeed;
        float movementZ = movementInput.y * movementSpeed;

        Vector3 horizontalMove = transform.right * movementX + transform.forward * movementZ;
        Vector3 move = Vector3.zero;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -1f;

            if (isJumping)
            {
                verticalVelocity = jumpForce;
                isJumping = false;
            }
        }
        else
        {
            verticalVelocity -= gravityForce * Time.deltaTime;
        }

        move = horizontalMove + Vector3.up * verticalVelocity;
        characterController.Move(move * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && characterController.isGrounded)
        {
            isJumping = true;
        }
    }
}