using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNew : MonoBehaviour
{

    private float force = 10f;
    private Rigidbody rb;
    private PlayerInput playerInput;
    public CharacterController controller;
    private Vector2 input;
    public float gravity = -20f;
    public float verticalVelocity;
    public float jumpHeight = 2f;
    private bool jumpPressed;



    void Start()
    {
       rb = GetComponent<Rigidbody>();
       playerInput = GetComponent<PlayerInput>();
       controller = GetComponent<CharacterController>();
    }

    
    void Update()
    {

        bool isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;

        }

        if (jumpPressed && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpPressed = false;
        
        }

        verticalVelocity += gravity * Time.deltaTime;

        input = playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0f, input.y).normalized;
        move.y = verticalVelocity / force;

        controller.Move(move * force * Time.deltaTime);

    }
    public void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();   
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(input.x, 0f, input.y) * force);
    }

    //public void Move(InputAction.CallbackContext callbackContext)
    //{
    //    if (callbackContext.performed)
    //    {
    //        Vector2 input = callbackContext.ReadValue<Vector2>();
    //        rb.AddForce(new Vector3(input.x, 0f, input.y) * force);
    //    }

    //}



}