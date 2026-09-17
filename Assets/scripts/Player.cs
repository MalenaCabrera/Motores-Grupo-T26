using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    [Header("Referencia de cámara")]
    public Transform cameraTransform;   // Arrastrar la Main Camera acá

    [Header("Velocidades de movimiento")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float crouchSpeed = 1.5f;
    public float rotationSmoothTime = 0.1f;

    [Header("Cámara orbital")]
    public float distance = 5.0f;
    public float height = 1.6f;
    public float rotationSpeedX = 150f;
    public float rotationSpeedY = 100f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private Rigidbody rb;
    private bool isCrouching;
    private bool isRunning;
    private float rotationVelocity;
    private float yaw;
    private float pitch = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        // Estados del jugador
        isCrouching = Input.GetKey(KeyCode.LeftControl);
        isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        // Input de la cámara (se mueve en Update para que sea fluido)
        yaw += Input.GetAxis("Mouse X") * rotationSpeedX * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeedY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);// probando
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Posiciona la cámara orbitando alrededor del player
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 focusPoint = transform.position + Vector3.up * height;
        Vector3 desiredPosition = focusPoint + rotation * Vector3.back * distance;

        cameraTransform.position = desiredPosition;
        cameraTransform.LookAt(focusPoint);
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDir.magnitude < 0.1f)
        {
            // Estar quieto: frena sin tocar la gravedad
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // Movimiento relativo a hacia dónde mira la cámara
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir = (camForward * vertical + camRight * horizontal).normalized;

        float currentSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);
        Vector3 velocity = moveDir * currentSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // Rotar el personaje hacia donde se mueve
        float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
    }

}
//public class Player : MonoBehaviour
//{
//    [Header("Configuración de Input")]
//    public InputActionAsset inputActions;

//    [Header("Referencias")]
//    public CharacterController playerCharacterController;
//    [SerializeField] private Transform playerCamera;

//    [Header("Movimiento")]
//    public float playerWalkSpeed = 5.0f;
//    public float playerRotateDampening = 0.1f;
//    private float turnSmoothingVelocity;

//    [Header("Físicas y Salto")]
//    public float jumpHeight = 5.0f;
//    private float gravity = 9.8f;
//    private float verticalVelocity = 0.0f;

//    private InputAction playerMoveAction;
//    private InputAction playerJumpAction;
//    private Vector2 playerMoveAmount;

//    private void Awake()
//    {
//        playerMoveAction = inputActions.FindActionMap("Player").FindAction("Move");
//        playerJumpAction = inputActions.FindActionMap("Player").FindAction("Jump");
//    }

//    private void OnEnable()
//    {
//        inputActions.FindActionMap("Player").Enable();
//    }

//    private void OnDisable()
//    {
//        inputActions.FindActionMap("Player").Disable();
//    }

//    private void Update()
//    {
//        playerMoveAmount = playerMoveAction.ReadValue<Vector2>();
//        HandleJump();
//        HandleMoveAndRotate();
//    }

//    private void HandleMoveAndRotate()
//    {
//        Vector3 playerDirection = new Vector3(playerMoveAmount.x, 0f, playerMoveAmount.y).normalized;
//        Vector3 verticalMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;

//        if (playerDirection.magnitude >= 0.1f)
//        {
//            // Calcula el ángulo según la cámara para rotar hacia donde mira
//            float targetAngle = Mathf.Atan2(playerDirection.x, playerDirection.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
//            float smoothTargetAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothingVelocity, playerRotateDampening);

//            transform.rotation = Quaternion.Euler(0f, smoothTargetAngle, 0f);

//            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
//            playerCharacterController.Move(moveDirection.normalized * playerWalkSpeed * Time.deltaTime + verticalMove);
//        }
//        else
//        {
//            playerCharacterController.Move(verticalMove);
//        }
//    }

//    private void HandleJump()
//    {
//        if (playerCharacterController.isGrounded)
//        {
//            verticalVelocity = -1f; // Fuerza hacia abajo constante para mantener contacto con el piso

//            if (playerJumpAction.WasPressedThisFrame())
//            {
//                verticalVelocity = jumpHeight;
//            }
//        }
//        else
//        {
//            verticalVelocity -= gravity * Time.deltaTime;
//        }
//    }
//}
