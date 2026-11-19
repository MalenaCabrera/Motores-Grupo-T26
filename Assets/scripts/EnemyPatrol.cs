using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Speed Between Waypoints")]
    [SerializeField] private float speed;

    //Here you set the Waypoints and "WaitTime" between Waypoints
    [Header("Waypoints/Rutines")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime;
    [SerializeField] private float rotationSpeed = 5.3f;
    private int currentWaypoint;
    private bool isWaiting;
    private bool isRotating;

    [Header("Persecución")]
    [SerializeField] private float velocidadPersecucion = 4.5f;
    private Transform targetJugador;
    private PlayerHealth vidaJugador; // reference to the health of the player we're chasing
    private bool estaPersiguiendo;
    private bool detenido;            // true después de DetenerPersecucion(): se queda quieto
    public bool IsChasing => estaPersiguiendo;
    [SerializeField] private string gameOverScene = "GameOverZombie1";

    [Header("Animator (opcional)")]
    [Tooltip("Si queda vacío se busca solo en el zombie o en sus hijos.")]
    [SerializeField] private Animator animator;
    [Tooltip("Qué tan rápido sigue el parámetro Speed a la velocidad real (más bajo = más suave).")]
    [SerializeField] private float animDampTime = 0.1f;
    [Tooltip("Segundos mínimos entre un disparo del trigger Attack y el siguiente.")]
    [SerializeField] private float attackAnimCooldown = 1.5f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private bool animHasSpeed;
    private bool animHasAttack;
    private Vector3 lastPosition;
    private float nextAttackAnimTime;

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            // El movimiento lo hace este script, no la animación.
            animator.applyRootMotion = false;

            if (animator.runtimeAnimatorController != null)
            {
                foreach (AnimatorControllerParameter p in animator.parameters)
                {
                    if (p.nameHash == SpeedHash && p.type == AnimatorControllerParameterType.Float) animHasSpeed = true;
                    if (p.nameHash == AttackHash && p.type == AnimatorControllerParameterType.Trigger) animHasAttack = true;
                }
            }
        }

        lastPosition = transform.position;
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            // Sin waypoints se queda quieto hasta que IniciarPersecucion() lo active.
            enabled = false;
            return;
        }
        Vector3 direction = (waypoints[0].position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        MoveLogic();
        UpdateAnimator();
    }

    private void MoveLogic()
    {
        //Después de frenarse (trigger de parada) no hace nada más
        if (detenido) return;

        //this line is placed at the top to ensure that the pursuit is prioritized over routine
        if (estaPersiguiendo)
        {
            PerseguirAlJugador();
            return;
        }

        //Do not move while waiting or while turning towards the next Waypoint
        if (isWaiting || isRotating) return;

        //If the enemy hasnt reached the current waypoint, move towards it
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].position, speed * Time.deltaTime);
        }
        //This start the Coroutine between Waypoints if waiting
        else if (!isWaiting)
        {
            StartCoroutine(Wait());
        }
    }

    // Alimenta el parámetro Speed con la velocidad horizontal REAL (metros por segundo),
    // igual que hace ZombieAI con el NavMeshAgent.
    private void UpdateAnimator()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        float realSpeed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
        lastPosition = transform.position;

        if (animator == null || !animHasSpeed) return;
        animator.SetFloat(SpeedHash, realSpeed, animDampTime, Time.deltaTime);
    }

    IEnumerator Wait()
    {
        isWaiting = true;
        //"waitTime" defines how long the enemy waits between each waypoint
        yield return new WaitForSeconds(waitTime);

        //Randomize nextWaypoint
        if (waypoints.Length == 1)
        {
            currentWaypoint = 0;
        }
        else
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
        isWaiting = false;
        StartCoroutine(RotBetweenWaypoints(waypoints[currentWaypoint].position));
    }

    //Smoothly rotates the enemy to face the next waypoint before it starts moving towards it
    IEnumerator RotBetweenWaypoints(Vector3 target)
    {
        isRotating = true;

        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isRotating = false;
    }

    public void IniciarPersecucion(Transform jugador, GameObject pared)
    {
        enabled = true;
        //stop all the coroutines
        StopAllCoroutines();
        isWaiting = false;
        isRotating = false;
        detenido = false;

        if (pared != null)
        {
            Destroy(pared);
            //(Aca se podrian agregar polvo o efectos mas adelante)
        }

        //Lock the target and start the chase
        targetJugador = jugador;
        vidaJugador = jugador.GetComponent<PlayerHealth>(); // look up the player's health once, when the chase starts
        estaPersiguiendo = true;
    }

    /// <summary>
    /// Corta la persecución y deja al enemigo quieto donde está.
    /// Lo llama el trigger de parada (ChaseStopTrigger).
    /// </summary>
    public void DetenerPersecucion()
    {
        if (!estaPersiguiendo) return;

        estaPersiguiendo = false;
        detenido = true;
        targetJugador = null;
        StopAllCoroutines();
        isWaiting = false;
        isRotating = false;
    }

    private void PerseguirAlJugador()
    {
        if (targetJugador == null) return;

        //If the player is already dead, stop moving and rotating entirely
        if (vidaJugador != null && vidaJugador.EstaMuerto)
        {
            return;
        }

        //this makes enemy to move in a straight line toward the player at running speed
        transform.position = Vector3.MoveTowards(transform.position, targetJugador.position, velocidadPersecucion * Time.deltaTime);

        //Rotate the plane on Y to face the player directly
        Vector3 direccion = (targetJugador.position - transform.position).normalized;
        direccion.y = 0;

        if (direccion != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionStay(Collision collision)
    {   //If the object the enemy collides with is the player, it kills him instantly
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerHealth componenteVidaJugador = collision.gameObject.GetComponent<PlayerHealth>();
        if (componenteVidaJugador != null)
        {
            // Animación de ataque (si el Animator tiene el trigger "Attack")
            if (animator != null && animHasAttack && Time.time >= nextAttackAnimTime && !componenteVidaJugador.EstaMuerto)
            {
                nextAttackAnimTime = Time.time + attackAnimCooldown;
                animator.SetTrigger(AttackHash);
            }

            componenteVidaJugador.Matar(gameOverScene);
        }
    }
}

