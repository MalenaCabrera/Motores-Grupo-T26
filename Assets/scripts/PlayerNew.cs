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
    [SerializeField] private Transform characterVisual;
    [SerializeField] private Vector3 standingScale = Vector3.one;
    [SerializeField] private Vector3 crouchScale = new Vector3(1f, 0.5f, 1f);
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
                    puerta.Interactuar(this);
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

        if (isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = -2f;
        }

        Vector2 inputMove = _playerInput.actions["Move"].ReadValue<Vector2>();

        float currentSpeed = walkSpeed;
        if (_playerInput.actions["Crouch"].IsPressed())
        {
            currentSpeed = crouchSpeed;
        }
        else if (_playerInput.actions["Sprint"].IsPressed())
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
        }

        _playerVelocity.y += gravity * Time.deltaTime;

        Vector3 finalMovement = (moveDirection * currentSpeed) + _playerVelocity;
        _char.Move(finalMovement * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (_playerInput.actions["Crouch"].IsPressed())
        {
            _char.height = crouchHeight;
            _char.center = new Vector3(_char.center.x, crouchCentery, _char.center.z);

            if (characterVisual != null)
            {
                characterVisual.localScale = crouchScale;
                characterVisual.localPosition = new Vector3(0f, crouchCentery, 0f);
            }
        }
        else
        {
            _char.height = standingHeight;
            _char.center = new Vector3(_char.center.x, standingCentery, _char.center.z);

            if (characterVisual != null)
            {
                characterVisual.localScale = standingScale;
                characterVisual.localPosition = new Vector3(0f, standingCentery, 0f);
            }
        }
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
        if (lfAngle > 360f) lfAngle += 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}

//using System.Collections.Generic;

//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(PlayerInput))]
//public class PlayerNew : MonoBehaviour
//{
//    [Header("Interacción")]
//    public float interactionDistance = 3f;
//    public LayerMask interactableLayer;

//    private List<string> _keys = new List<string>();

//    private CharacterController _char;
//    private PlayerInput _playerInput;
//    private Transform _cameraTransform;

//    [Header("Player Settings")]
//    [SerializeField] private float walkSpeed = 4f;
//    [SerializeField] private float sprintSpeed = 7f;
//    [SerializeField] private float jumpHeight = 1.2f;
//    [SerializeField] private float turnSmoothTime = 0.1f;
//    [SerializeField] private float gravity = -15f;

//    [SerializeField] private float crouchSpeed = 2f;
//    [SerializeField] private float standingHeight = 2f;
//    [SerializeField] private float crouchHeight = 1f;
//    [SerializeField] private float standingCentery = 1f;
//    [SerializeField] private float crouchCentery = 0.5f;
//    [SerializeField] private Transform characterVisual;
//    [SerializeField] private Vector3 standingScale = Vector3.one;
//    [SerializeField] private Vector3 crouchScale = new Vector3(1f, 0.5f, 1f);
//    private Vector3 _playerVelocity;
//    private float _turnSmoothVelocity;

//    [Header("Cinemachine Settings")]
//    [SerializeField] private GameObject cinemachineCameraTarget;
//    [SerializeField] private float sensitivity = 1f;
//    [SerializeField] private float bottomClamp = -30f;
//    [SerializeField] private float topClamp = 70f;

//    private float _cinemachineTargetYaw;
//    private float _cinemachineTargetPitch;

//    private void Start()
//    {
//        _char = GetComponent<CharacterController>();
//        _playerInput = GetComponent<PlayerInput>();

//        if (Camera.main != null)
//        {
//            _cameraTransform = Camera.main.transform;
//        }

//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;

//        _cinemachineTargetYaw = transform.eulerAngles.y;
//    }

//    private void Update()
//    {
//        HandleCrouch();
//        HandleMovementAndGravity();
//        if (Input.GetKeyDown(KeyCode.E) && _cameraTransform != null)
//        {
//            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
//            RaycastHit hit;

//            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
//            {
//                // Primero comprobar si es una llave
//                KeyItem llave = hit.collider.GetComponentInParent<KeyItem>();

//                if (llave != null)
//                {
//                    llave.Recoger(this);
//                    return;
//                }

//                // Después comprobar si es una puerta
//                DoorRange puerta = hit.collider.GetComponentInParent<DoorRange>();

//                if (puerta != null)
//                {
//                    puerta.Interactuar(this);
//                    return;
//                }
//            }
//        }


//    }

//    public void AgregarLlave(string keyID)
//    {
//        if (!_keys.Contains(keyID))
//        {
//            _keys.Add(keyID);
//            Debug.Log("Llave guardada en el jugador: " + keyID);
//        }
//    }

//    public bool TieneLlave(string keyID)
//    {
//        return _keys.Contains(keyID);
//    }

//    public void RemoverLlave(string keyID)
//    {
//        if (_keys.Contains(keyID))
//        {
//            _keys.Remove(keyID);
//        }
//    }

//    private void LateUpdate()
//    {
//        CameraRotation();
//    }

//    private void HandleMovementAndGravity()
//    {
//        bool isGrounded = _char.isGrounded;

//        if (isGrounded && _playerVelocity.y < 0)
//        {
//            _playerVelocity.y = -2f;
//        }

//        Vector2 inputMove = _playerInput.actions["Move"].ReadValue<Vector2>();

//        float currentSpeed = walkSpeed;
//        if (_playerInput.actions["Crouch"].IsPressed())
//        {
//            currentSpeed = crouchSpeed;
//        }
//        else if (_playerInput.actions["Sprint"].IsPressed())
//        {
//            currentSpeed = sprintSpeed;
//        }

//        Vector3 moveDirection = Vector3.zero;

//        if (inputMove.magnitude >= 0.1f && _cameraTransform != null)
//        {
//            Vector3 camForward = _cameraTransform.forward;
//            Vector3 camRight = _cameraTransform.right;
//            camForward.y = 0f;
//            camRight.y = 0f;
//            camForward.Normalize();
//            camRight.Normalize();

//            moveDirection = (camForward * inputMove.y + camRight * inputMove.x).normalized;

//            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
//            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
//            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
//        }

//        if (_playerInput.actions["Jump"].WasPressedThisFrame() && isGrounded)
//        {
//            _playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
//        }

//        _playerVelocity.y += gravity * Time.deltaTime;

//        Vector3 finalMovement = (moveDirection * currentSpeed) + _playerVelocity;
//        _char.Move(finalMovement * Time.deltaTime);
//    }

//    private void HandleCrouch()
//    {
//        if (_playerInput.actions["Crouch"].IsPressed())
//        {
//            _char.height = crouchHeight;
//            _char.center = new Vector3(_char.center.x, crouchCentery, _char.center.z);

//            if (characterVisual != null)
//            {
//                characterVisual.localScale = crouchScale;
//                characterVisual.localPosition = new Vector3(0f, crouchCentery, 0f);
//            }
//        }
//        else
//        {
//            _char.height = standingHeight;
//            _char.center = new Vector3(_char.center.x, standingCentery, _char.center.z);

//            if (characterVisual != null)
//            {
//                characterVisual.localScale = standingScale;
//                characterVisual.localPosition = new Vector3(0f, standingCentery, 0f);
//            }
//        }
//    }

//    private void CameraRotation()
//    {
//        if (cinemachineCameraTarget == null) return;

//        Vector2 inputLook = _playerInput.actions["Look"].ReadValue<Vector2>();

//        if (inputLook.sqrMagnitude >= 0.01f)
//        {
//            _cinemachineTargetYaw += inputLook.x * Time.deltaTime * (sensitivity * 10f);
//            _cinemachineTargetPitch += -inputLook.y * Time.deltaTime * (sensitivity * 10f);
//        }

//        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
//        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0f);
//    }

//    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
//    {
//        if (lfAngle < -360f) lfAngle += 360f;
//        if (lfAngle > 360f) lfAngle -= 360f;
//        return Mathf.Clamp(lfAngle, lfMin, lfMax);
//    }
//}
