using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerNew : MonoBehaviour
{
    [Header("Interacción")]
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    [Header("Inventario")]
    [SerializeField] private Inventory inventario;
    public Inventory Inventario => inventario;

    private CharacterController _char;
    private PlayerInput _playerInput;
    private Transform _cameraTransform;

    [Header("Player Settings")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float gravity = -15f;

    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingCentery = 1f;
    [SerializeField] private float crouchCentery = 0.5f;
    [SerializeField] private float colliderRadius = 0.3f;
    private Vector3 _playerVelocity;
    private float _turnSmoothVelocity;

    // ---- Estado que lee PlayerAnimation (solo lectura desde afuera) ----
    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsSprinting { get; private set; }
    public float HorizontalSpeed { get; private set; }   // velocidad real en m/s (0 = quieto)
    public float VerticalVelocity => _playerVelocity.y;

    public event System.Action OnJumped;                 // se dispara al saltar
    public event System.Action<bool> OnDoorInteracted;   // true = abrió, false = cerró

    [Header("Cinemachine Settings")]
    [SerializeField] private GameObject cinemachineCameraTarget;
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private float bottomClamp = -30f;
    [SerializeField] private float topClamp = 70f;

    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private void Start()
    {
        _char = GetComponent<CharacterController>();
        _char.radius = colliderRadius;
        _playerInput = GetComponent<PlayerInput>();

        if (inventario == null)
        {
            inventario = GetComponent<Inventory>();
        }

        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _cinemachineTargetYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleCrouch();
        HandleMovementAndGravity();

        if (Input.GetKeyDown(KeyCode.E) && _cameraTransform != null)
        {
            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
                KeyItem llave = hit.collider.GetComponentInParent<KeyItem>();
                if (llave != null)
                {
                    llave.Recoger(this);
                    return;
                }

                DoorRange puerta = hit.collider.GetComponentInParent<DoorRange>();
                if (puerta != null)
                {
                    bool estabaAbierta = puerta.IsOpen;
                    puerta.Interactuar(this);

                    // Solo animamos si la puerta cambió de estado (una trabada no dispara nada)
                    if (puerta.IsOpen != estabaAbierta)
                    {
                        OnDoorInteracted?.Invoke(puerta.IsOpen);
                    }
                    return;
                }
            }
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void HandleMovementAndGravity()
    {
        bool isGrounded = _char.isGrounded;
        IsGrounded = isGrounded;

        if (isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }

        Vector2 inputMove = _playerInput.actions["Move"].ReadValue<Vector2>();

        bool wantsCrouch = _playerInput.actions["Crouch"].IsPressed();
        bool wantsSprint = _playerInput.actions["Sprint"].IsPressed() && !wantsCrouch;

        float currentSpeed = walkSpeed;
        if (wantsCrouch)
        {
            currentSpeed = crouchSpeed;
        }
        else if (wantsSprint)
        {
            currentSpeed = sprintSpeed;
        }

        Vector3 moveDirection = Vector3.zero;

        if (inputMove.magnitude >= 0.1f && _cameraTransform != null)
        {
            Vector3 camForward = _cameraTransform.forward;
            Vector3 camRight = _cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * inputMove.y + camRight * inputMove.x).normalized;

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        if (_playerInput.actions["Jump"].WasPressedThisFrame() && isGrounded)
        {
            _playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            OnJumped?.Invoke();
        }

        _playerVelocity.y += gravity * Time.deltaTime;

        // Datos para el Animator (0 si no hay input, así vuelve a Idle)
        HorizontalSpeed = moveDirection.sqrMagnitude > 0f ? currentSpeed : 0f;
        IsSprinting = wantsSprint && HorizontalSpeed > 0f;

        Vector3 finalMovement = (moveDirection * currentSpeed) + _playerVelocity;
        _char.Move(finalMovement * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        IsCrouching = _playerInput.actions["Crouch"].IsPressed();

        // El agachado visual lo hace la animación "Crouched Walking" (Animator, vía PlayerAnimation).
        // Acá solo se achica el CharacterController para poder pasar por lugares bajos.
        _char.height = IsCrouching ? crouchHeight : standingHeight;
        _char.center = new Vector3(_char.center.x, IsCrouching ? crouchCentery : standingCentery, _char.center.z);
    }

    private void CameraRotation()
    {
        if (cinemachineCameraTarget == null) return;

        Vector2 inputLook = _playerInput.actions["Look"].ReadValue<Vector2>();

        if (inputLook.sqrMagnitude >= 0.01f)
        {
            _cinemachineTargetYaw += inputLook.x * Time.deltaTime * (sensitivity * 10f);
            _cinemachineTargetPitch += -inputLook.y * Time.deltaTime * (sensitivity * 10f);
        }

        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
