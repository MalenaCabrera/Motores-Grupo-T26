using UnityEngine;

/// <summary>
/// Conecta PlayerNew con el Animator del personaje.
/// Poner este script en el GameObject "Player" (el mismo que tiene PlayerNew).
/// El Animator va en el modelo hijo; si no se asigna a mano, se busca solo.
/// </summary>
[RequireComponent(typeof(PlayerNew))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;

    [Header("Suavizado")]
    [Tooltip("Tiempo para suavizar el parámetro Speed (evita saltos bruscos Idle -> Walk -> Run)")]
    [SerializeField] private float speedDampTime = 0.1f;

    // Los nombres tienen que ser IDÉNTICOS a los parámetros del Animator Controller.
    // Se guardan como hash (int) porque es más rápido que usar strings en cada frame.
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VerticalVelHash = Animator.StringToHash("VerticalVelocity");
    private static readonly int CrouchMoveHash = Animator.StringToHash("CrouchMoveMult");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int OpenDoorHash = Animator.StringToHash("OpenDoor");
    private static readonly int CloseDoorHash = Animator.StringToHash("CloseDoor");

    private PlayerNew player;

    private void Awake()
    {
        player = GetComponent<PlayerNew>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("[PlayerAnimation] No encontré un Animator. Agregalo al modelo del personaje.", this);
            enabled = false;
            return;
        }

        // El movimiento lo hace el CharacterController, NO las animaciones.
        // Si esto queda en true el personaje se desplaza doble o se queda clavado.
        animator.applyRootMotion = false;
    }

    private void OnEnable()
    {
        player.OnJumped += HandleJump;
        player.OnDoorInteracted += HandleDoor;
    }

    private void OnDisable()
    {
        player.OnJumped -= HandleJump;
        player.OnDoorInteracted -= HandleDoor;
    }

    private void Update()
    {
        // Speed va en m/s reales: 0 = Idle, ~4 = Walk, ~7 = Run (los umbrales del Blend Tree)
        animator.SetFloat(SpeedHash, player.HorizontalSpeed, speedDampTime, Time.deltaTime);

        animator.SetBool(IsCrouchingHash, player.IsCrouching);
        animator.SetBool(IsGroundedHash, player.IsGrounded);
        animator.SetFloat(VerticalVelHash, player.VerticalVelocity);

        // Mixamo solo trae "Crouched Walking" (no hay agachado quieto).
        // Con este multiplicador la animación se congela cuando no te movés.
        animator.SetFloat(CrouchMoveHash, player.HorizontalSpeed > 0.1f ? 1f : 0f);
    }

    private void HandleJump()
    {
        animator.SetTrigger(JumpHash);
    }

    private void HandleDoor(bool opened)
    {
        // Se resetea el trigger opuesto para que no quede uno "colgado" si se spamea la E
        animator.ResetTrigger(opened ? CloseDoorHash : OpenDoorHash);
        animator.SetTrigger(opened ? OpenDoorHash : CloseDoorHash);
    }
}

