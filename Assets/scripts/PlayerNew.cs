using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerNew : MonoBehaviour
{
    private CharacterController _char;
    private PlayerInput _playerInput;
    private Transform _cameraTransform;

    [Header("Player Settings")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float gravity = -15f;

    private Vector3 _playerVelocity;
    private float _turnSmoothVelocity;

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
        _playerInput = GetComponent<PlayerInput>();

        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        // Bloquear cursor en pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _cinemachineTargetYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleMovementAndGravity();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void HandleMovementAndGravity()
    {
        bool isGrounded = _char.isGrounded;

        if (isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }

        // 1. Lectura directa del New Input System (como el profe)
        Vector2 inputMove = _playerInput.actions["Move"].ReadValue<Vector2>();
        bool isSprinting = _playerInput.actions["Sprint"].IsPressed();

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // 2. Movimiento relativo a hacia dónde mira la cámara
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

            // Rotar al muñeco suavemente hacia la dirección del movimiento
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }

        // 3. Salto (WasPressedThisFrame evita saltos dobles)
        if (_playerInput.actions["Jump"].WasPressedThisFrame() && isGrounded)
        {
            _playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. Gravedad
        _playerVelocity.y += gravity * Time.deltaTime;

        // 5. Aplicar todo el movimiento junto en una sola llamada
        Vector3 finalMovement = (moveDirection * currentSpeed) + _playerVelocity;
        _char.Move(finalMovement * Time.deltaTime);
    }

    private void CameraRotation()
    {
        if (cinemachineCameraTarget == null) return;

        // Lectura del mouse directamente desde la acción "Look"
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

