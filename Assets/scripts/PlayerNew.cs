using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(CharacterController))]
public class PlayerNew : MonoBehaviour
{
    private CharacterController _char;
    private PlayerInputs _inputs;

    [Header("Player references")]

    [SerializeField] private float playerSpeed = 4f;

    [SerializeField] private float playerJump = 1f;

    [SerializeField] private float playerTurnSpeed = 0.1f;

    [SerializeField] private float playerGravity = -9.8f;

    private Vector3 dir;

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    [Header("Cinemachine references")]

    [SerializeField] private GameObject cinemachineCamera;

    [SerializeField] private GameObject cinemachineCameraTarget;

    [SerializeField] private float bottomClamp = -30f;

    [SerializeField] private float topClamp = 70f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private Transform cameraTransform;

    private float turnSmootVelocity;


    private void Start()
    {
        _char = GetComponent<CharacterController>();

        _inputs = GetComponent<PlayerInputs>();

        cameraTransform = Camera.main.transform;

        // La cámara comienza mirando en la misma dirección que el Player
        _cinemachineTargetYaw = transform.eulerAngles.y;
    }


    private void Update()
    {
        // Movimiento + gravedad + salto
        GravityAndJump();
        Movement();
    }


    private void LateUpdate()
    {
        // Cámara
        CameraRotation();
    }


    // =====================================================
    // GRAVEDAD Y SALTO
    // =====================================================

    private void GravityAndJump()
    {
        groundedPlayer = _char.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }


        // SALTO

        if (_inputs.jump && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(playerJump * (-2f * playerGravity));

            _inputs.jump = false;
        }


        // GRAVEDAD

        playerVelocity.y +=
            playerGravity * Time.deltaTime;


        // MOVIMIENTO VERTICAL

        _char.Move(
            playerVelocity * Time.deltaTime
        );


        // MOVIMIENTO HORIZONTAL

        Movement();
    }


    // =====================================================
    // MOVIMIENTO RELATIVO A LA CÁMARA
    // =====================================================

    private void Movement()
    {
        Vector3 movement =
            new Vector3(_inputs.move.x,0f, _inputs.move.y);


        if (movement.magnitude < 0.15f)
        {
            return;
        }


        // Dirección de la cámara

        Vector3 cameraForward = cameraTransform.forward;

        Vector3 cameraRight = cameraTransform.right;


        // Ignoramos la inclinación vertical

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();


        // Movimiento relativo a la cámara

        dir = cameraForward * _inputs.move.y + cameraRight * _inputs.move.x;


        dir.Normalize();


        // Ángulo hacia donde debe mirar el Player

        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;


        // Rotación suave

        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmootVelocity, playerTurnSpeed);


        transform.rotation = Quaternion.Euler( 0f, angle, 0f);


        // Movimiento

        _char.Move(dir * playerSpeed * Time.deltaTime );
    }


    // =====================================================
    // CÁMARA
    // =====================================================

    private void CameraRotation()
    {
        if (_inputs.look.sqrMagnitude >= 0.01f)
        {
            // Mouse X

            _cinemachineTargetYaw += _inputs.look.x * Time.deltaTime * (_inputs.playerSensitivity * 10f);


            // Mouse Y

            _cinemachineTargetPitch += -_inputs.look.y * Time.deltaTime * (_inputs.playerSensitivity * 10f);
        }


        // Limitar arriba / abajo

        _cinemachineTargetPitch = ClampAngle( _cinemachineTargetPitch, bottomClamp, topClamp );


        // Aplicar la rotación al Target

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler( _cinemachineTargetPitch, _cinemachineTargetYaw, 0f );

    }


    // =====================================================
    // CLAMP
    // =====================================================

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f)lfAngle += 360f;

        if (lfAngle > 360f)lfAngle -= 360f;

        return Mathf.Clamp(lfAngle, lfMin, lfMax);

    }
}



//[RequireComponent(typeof(CharacterController))]
//public class PlayerNew : MonoBehaviour
//{
//    private CharacterController _char;
//    private PlayerInputs _inputs;

//    [Header("Player references")]

//    [SerializeField] private float playerSpeed = 4f;

//    [SerializeField] private float playerJump = 1f;

//    [SerializeField] private float playerTurnSpeed = 1f;

//    [SerializeField] private float playerGravity = -9.8f;

//    public float playerSensitivity = 1f;

//    private Vector3 dir;

//    private Vector3 playerVelocity;
//    private bool groundedPlayer;

//    [Header("Cinemachine references")]
//    [SerializeField] private GameObject cinemachineCamera;
//    [SerializeField] private GameObject cinemachineCameraTarget;
//    [SerializeField] private float bottomClamp = -30f;
//    [SerializeField] private float topClamp = 90f;
//    private float _cinemachineTargetYaw, _cinemachineTargetPitch;

//    private Transform cameraTransform;
//    private float turnSmootVelocity;
//    private float targetAngle;

//    private void Start()
//    {
//        _char = GetComponent<CharacterController>();
//        _inputs = GetComponent<PlayerInputs>();
//        cameraTransform = Camera.main.transform;
//    }


//    private void Update()
//    {
//        //Movement();
//    }


//    private void LateUpdate()
//    {
//        cameraRotation();
//    }

//    private void GravityAndJump()
//    {
//        groundedPlayer = _char.isGrounded;
//        if(groundedPlayer && playerVelocity.y <0)
//            playerVelocity.y = 0f;


//        Movement();
        
//        if(_inputs.jump && groundedPlayer)
//        {
//            playerVelocity.y += Mathf.Sqrt(playerJump * (-3.0f * playerGravity));
//            _inputs.jump = false;
//        }
//        playerVelocity.y += playerGravity * Time.deltaTime;
//        _char.Move(playerVelocity * Time.deltaTime);
//    }

//    private void Movement()
//    {
//        Vector3 movement = new Vector3(_inputs.move.x, 0f, _inputs.move.y);

//        if(movement.magnitude >= 0.15f)
//        {
//            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
//            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmootVelocity, playerTurnSpeed);

//            transform.rotation = Quaternion.Euler(0f, angle, 0f);
//            dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

//            _char.Move(dir * playerSpeed * Time.deltaTime);
//        }
//    }


//    private void cameraRotation()
//    {
//        if (_inputs.look.sqrMagnitude >= 0.1f)
//        {
//            _cinemachineTargetYaw += _inputs.look.x * Time.deltaTime * (_inputs.playerSensitivity * 10);
//            _cinemachineTargetPitch += -_inputs.look.y * Time.deltaTime * (_inputs.playerSensitivity * 10);

//        }

//        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
//        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);


//    }

//    private static float ClampAngle(float lfAngle, float lfMin, float lfMax )
//    {
//        if (lfAngle < -360f) lfAngle += 360f;
//        if (lfAngle > 360f) lfAngle -= 360f;
//        return Mathf.Clamp(lfAngle, lfMin, lfMax);
//    }













    //private PlayerInput playerInput;
    //public CharacterController controller;

    //private Vector2 input;

    //[Header("Cámara")]
    //public Transform cameraTransform;

    //[Header("Movimiento")]
    //public float moveSpeed = 5f;
    //public float rotationSpeed = 10f;

    //[Header("Gravedad")]
    //public float gravity = -20f;
    //private float verticalVelocity;

    //[Header("Salto")]
    //public float jumpHeight = 2f;
    //private bool jumpPressed;


    //void Start()
    //{
    //    playerInput = GetComponent<PlayerInput>();
    //    controller = GetComponent<CharacterController>();
    //}


    //void Update()
    //{
    //    // =====================================
    //    // INPUT DE MOVIMIENTO
    //    // =====================================

    //    input = playerInput.actions["Move"].ReadValue<Vector2>();


    //    // =====================================
    //    // DIRECCIÓN DE LA CÁMARA
    //    // =====================================

    //    Vector3 cameraForward = cameraTransform.forward;
    //    Vector3 cameraRight = cameraTransform.right;

    //    // Eliminamos la inclinación vertical
    //    cameraForward.y = 0f;
    //    cameraRight.y = 0f;

    //    cameraForward.Normalize();
    //    cameraRight.Normalize();


    //    // =====================================
    //    // MOVIMIENTO RELATIVO A LA CÁMARA
    //    // =====================================

    //    Vector3 move = cameraForward * input.y + cameraRight * input.x;

    //    // Evita que la diagonal sea más rápida
    //    if (move.magnitude > 1f)
    //    {
    //        move.Normalize();
    //    }


    //    // =====================================
    //    // GIRAR EL PLAYER HACIA EL MOVIMIENTO
    //    // =====================================

    //    if (move.magnitude > 0.1f)
    //    {
    //        Quaternion targetRotation = Quaternion.LookRotation(move);

    //        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    //    }


    //    // =====================================
    //    // GRAVEDAD
    //    // =====================================

    //    bool isGrounded = controller.isGrounded;

    //    if (isGrounded && verticalVelocity < 0)
    //    {
    //        verticalVelocity = -2f;
    //    }


    //    // =====================================
    //    // SALTO
    //    // =====================================

    //    if (jumpPressed && isGrounded)
    //    {
    //        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

    //        jumpPressed = false;
    //    }

    //    verticalVelocity += gravity * Time.deltaTime;


    //    // =====================================
    //    // MOVIMIENTO FINAL
    //    // =====================================

    //    Vector3 finalMove = move * moveSpeed;

    //    finalMove.y = verticalVelocity;

    //    controller.Move(finalMove * Time.deltaTime);
    //}


    //// =====================================
    //// INPUT SYSTEM
    //// =====================================

    //public void OnMove(InputValue value)
    //{
    //    input = value.Get<Vector2>();
    //}


    //public void OnJump(InputValue value)
    //{
    //    if (value.isPressed)
    //    {
    //        jumpPressed = true;
    //    }
    //}
//}




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
