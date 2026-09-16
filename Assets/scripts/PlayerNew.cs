using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNew : MonoBehaviour
{
    private PlayerInput playerInput;
    public CharacterController controller;

    private Vector2 input;

    [Header("Cámara")]
    public Transform cameraTransform;

    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Gravedad")]
    public float gravity = -20f;
    private float verticalVelocity;

    [Header("Salto")]
    public float jumpHeight = 2f;
    private bool jumpPressed;


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<CharacterController>();
    }


    void Update()
    {
        // =====================================
        // INPUT DE MOVIMIENTO
        // =====================================

        input = playerInput.actions["Move"].ReadValue<Vector2>();


        // =====================================
        // DIRECCIÓN DE LA CÁMARA
        // =====================================

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        // Eliminamos la inclinación vertical
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();


        // =====================================
        // MOVIMIENTO RELATIVO A LA CÁMARA
        // =====================================

        Vector3 move = cameraForward * input.y + cameraRight * input.x;

        // Evita que la diagonal sea más rápida
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }


        // =====================================
        // GIRAR EL PLAYER HACIA EL MOVIMIENTO
        // =====================================

        if (move.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }


        // =====================================
        // GRAVEDAD
        // =====================================

        bool isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }


        // =====================================
        // SALTO
        // =====================================

        if (jumpPressed && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpPressed = false;
        }

        verticalVelocity += gravity * Time.deltaTime;


        // =====================================
        // MOVIMIENTO FINAL
        // =====================================

        Vector3 finalMove = move * moveSpeed;

        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }


    // =====================================
    // INPUT SYSTEM
    // =====================================

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
}




//public class PlayerNew : MonoBehaviour
//{

//    private float force = 10f;
//    //private Rigidbody rb;
//    private PlayerInput playerInput;
//    public CharacterController controller;
//    private Vector2 input;
//    private Vector2 look;
//    public float mouseSensitivity = 0.1f;
//    public Transform cameraTransform;
//    public float gravity = -20f;
//    public float verticalVelocity;
//    public float jumpHeight = 2f;
//    private bool jumpPressed;

    

//    void Start()
//    {
//       //rb = GetComponent<Rigidbody>();
//       playerInput = GetComponent<PlayerInput>();
//       controller = GetComponent<CharacterController>();
//    }

    
//    void Update()
//    {




//        bool isGrounded = controller.isGrounded;
//        if (isGrounded && verticalVelocity < 0)
//        {
//            verticalVelocity = -2f;

//        }

//        if (jumpPressed && isGrounded)
//        {
//            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
//                jumpPressed = false;
        
//        }

//        verticalVelocity += gravity * Time.deltaTime;

//        input = playerInput.actions["Move"].ReadValue<Vector2>();
//        look = playerInput.actions["Look"].ReadValue<Vector2>();
//        transform.Rotate(0f, look.x * mouseSensitivity, 0f);

//        Vector3 move = new Vector3(input.x, 0f, input.y).normalized;
//        move.y = verticalVelocity / force;

//        controller.Move(move * force * Time.deltaTime);

//    }
//    public void OnMove(InputValue value)
//    {
//        input = value.Get<Vector2>();   
//    }

//    public void OnJump(InputValue value)
//    {
//        if (value.isPressed)
//        {
//            jumpPressed = true;
//        }
//    }

    //private void FixedUpdate()
    //{
    //    rb.AddForce(new Vector3(input.x, 0f, input.y) * force);





    //}

 


