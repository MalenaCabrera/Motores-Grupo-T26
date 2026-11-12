using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// IA del Skinless Zombie:
///  - RONDA: camina entre puntos al azar dentro de una zona (radio alrededor de un centro).
///  - PERSIGUE: si el Player entra en su rango de visión (distancia + ángulo + sin paredes en el medio).
///  - Si pierde al Player, va hasta donde lo vio por última vez y, pasado un tiempo, vuelve a rondar.
///
/// Poner en el objeto raíz del zombie junto con un NavMeshAgent.
/// Requiere un NavMesh horneado en la escena (NavMeshSurface > Bake).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    private enum State { Patrol, Chase }

    [Header("Referencias (opcionales)")]
    [Tooltip("Si queda vacío, busca solo al objeto que tenga PlayerNew.")]
    [SerializeField] private Transform player;
    [Tooltip("Si queda vacío, lo busca en los hijos.")]
    [SerializeField] private Animator animator;

    [Header("Zona de ronda")]
    [Tooltip("Centro de la zona. Si queda vacío, usa el punto donde está el zombie al empezar.")]
    [SerializeField] private Transform zoneCenter;
    [SerializeField] private float zoneRadius = 8f;
    [Tooltip("Segundos que se queda quieto al llegar a cada punto (mínimo, máximo).")]
    [SerializeField] private Vector2 waitTime = new Vector2(2f, 5f);
    [SerializeField] private float patrolSpeed = 1f;

    [Header("Visión")]
    [SerializeField] private float visionRange = 10f;
    [Tooltip("Ángulo total del cono de visión, hacia adelante.")]
    [SerializeField, Range(10f, 360f)] private float visionAngle = 120f;
    [Tooltip("A esta distancia o menos te detecta aunque estés a sus espaldas.")]
    [SerializeField] private float closeDetectRange = 2f;
    [Tooltip("Altura de los ojos del zombie.")]
    [SerializeField] private float eyeHeight = 1.6f;
    [Tooltip("Altura del punto del Player al que apunta (el pecho).")]
    [SerializeField] private float targetHeight = 1f;
    [Tooltip("Qué capas bloquean la vista (paredes, puertas, muebles).")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [Tooltip("Si estás escondido (agachado en una HideZone), a esta distancia o menos igual te detecta. " +
             "0 = nunca te ve mientras estés escondido.")]
    [SerializeField] private float hiddenDetectRange = 0f;

    [Header("Persecución")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [Tooltip("Mientras persigue, te sigue viendo hasta rango x este valor (aunque estés de espaldas).")]
    [SerializeField] private float chaseRangeMultiplier = 1.5f;
    [Tooltip("Segundos que sigue buscando después de perderte de vista, antes de volver a rondar.")]
    [SerializeField] private float loseSightTime = 4f;

    [Header("Ataque")]
    [SerializeField] private bool canAttack = true;
    [SerializeField] private float attackRange = 1.6f;
    [SerializeField] private float attackDamage = 34f;
    [SerializeField] private float attackCooldown = 1.5f;
    [Tooltip("Si está activo, el daño se aplica cuando la animación de ataque llama a OnAttackHit() " +
             "(Animation Event en el frame del golpe). Si está apagado, se aplica apenas arranca el ataque.")]
    [SerializeField] private bool damageOnAnimationEvent = true;
    [Tooltip("Margen extra de distancia en el momento del golpe. Si te alejaste más que Attack Range + este valor, el golpe falla.")]
    [SerializeField] private float hitRangeMargin = 0.5f;

    // Parámetros del Animator (opcionales: si no existen, simplemente no se usan).
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private NavMeshAgent agent;
    private PlayerHealth playerHealth;
    private PlayerNew playerScript;
    private State state = State.Patrol;
    private Vector3 home;
    private bool animHasSpeed;
    private bool animHasAttack;

    private bool waiting;
    private float waitTimer;
    private float lostTimer;
    private float repathTimer;
    private float nextAttackTime;
    private bool pendingHit;
    private Vector3 lastKnownPlayerPos;

    private readonly RaycastHit[] hits = new RaycastHit[16];
    private const float PatrolStoppingDistance = 0.3f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // El movimiento lo hace el NavMeshAgent, no las animaciones.
            animator.applyRootMotion = false;
        }

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.nameHash == SpeedHash && p.type == AnimatorControllerParameterType.Float) animHasSpeed = true;
                if (p.nameHash == AttackHash && p.type == AnimatorControllerParameterType.Trigger) animHasAttack = true;
            }
        }
    }

    private void Start()
    {
        home = zoneCenter != null ? zoneCenter.position : transform.position;

        if (player == null)
        {
            PlayerNew p = FindFirstObjectByType<PlayerNew>();
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponentInParent<PlayerHealth>();
            playerScript = player.GetComponentInParent<PlayerNew>();
        }
        else Debug.LogWarning("[ZombieAI] No encontré al Player en la escena.", this);

        agent.speed = patrolSpeed;
        agent.stoppingDistance = PatrolStoppingDistance;

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("[ZombieAI] El zombie no está sobre un NavMesh. Horneá el NavMesh (NavMeshSurface > Bake) " +
                           "y revisá que el zombie esté apoyado en el piso.", this);
            enabled = false;
            return;
        }

        PickPatrolPoint();
    }

    private void Update()
    {
        if (player == null) return;

        // Si el Player murió, el zombie se frena.
        if (playerHealth != null && playerHealth.EstaMuerto)
        {
            agent.isStopped = true;
            UpdateAnimator();
            return;
        }

        switch (state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
        }

        UpdateAnimator();
    }

    // ---------------------------------------------------------------- RONDA

    private void UpdatePatrol()
    {
        if (CanSeePlayer(false))
        {
            StartChase();
            return;
        }

        if (agent.pathPending) return;

        bool arrived = !agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        if (!arrived) return;

        if (!waiting)
        {
            waiting = true;
            waitTimer = Random.Range(waitTime.x, waitTime.y);
            return;
        }

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            waiting = false;
            PickPatrolPoint();
        }
    }

    /// <summary>Elige un punto al azar dentro de la zona al que el zombie realmente pueda llegar.</summary>
    private void PickPatrolPoint()
    {
        Vector3 center = zoneCenter != null ? zoneCenter.position : home;
        NavMeshPath path = new NavMeshPath();

        for (int i = 0; i < 10; i++)
        {
            Vector2 offset = Random.insideUnitCircle * zoneRadius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 2f, NavMesh.AllAreas)) continue;
            if (!agent.CalculatePath(navHit.position, path)) continue;
            if (path.status != NavMeshPathStatus.PathComplete) continue;

            agent.SetPath(path);
            return;
        }
        // Si no encontró ninguno, reintenta después de la próxima espera.
    }

    // ---------------------------------------------------------- PERSECUCIÓN

    private void StartChase()
    {
        state = State.Chase;
        waiting = false;
        lostTimer = 0f;
        repathTimer = 0f;
        lastKnownPlayerPos = player.position;

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = canAttack ? Mathf.Max(PatrolStoppingDistance, attackRange * 0.8f) : PatrolStoppingDistance;
    }

    private void StopChase()
    {
        state = State.Patrol;
        waiting = false;

        agent.isStopped = false;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = PatrolStoppingDistance;

        PickPatrolPoint();
    }

    private void UpdateChase()
    {
        bool sees = CanSeePlayer(true);

        if (sees)
        {
            lostTimer = 0f;
            lastKnownPlayerPos = player.position;
        }
        else
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= loseSightTime)
            {
                StopChase();
                return;
            }
        }

        // Ataque cuerpo a cuerpo
        if (canAttack && sees && PlanarDistanceTo(player.position) <= attackRange)
        {
            agent.isStopped = true;
            FaceTarget(player.position);
            TryAttack();
            return;
        }

        agent.isStopped = false;
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = 0.15f;
            agent.SetDestination(sees ? player.position : lastKnownPlayerPos);
        }
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        bool hasAttackAnim = animator != null && animHasAttack;
        if (hasAttackAnim) animator.SetTrigger(AttackHash);

        if (damageOnAnimationEvent && hasAttackAnim)
            pendingHit = true;      // el daño lo aplica OnAttackHit(), en el frame del golpe
        else
            ApplyDamage();          // sin animación de ataque: daño inmediato
    }

    /// <summary>
    /// La llama el Animation Event del clip de ataque, en el frame en que el brazo golpea.
    /// Tiene que ser public, sin parámetros, y el script tiene que estar en el mismo objeto que el Animator.
    /// </summary>
    public void OnAttackHit()
    {
        if (!pendingHit) return;    // evita golpes dobles o eventos sueltos
        pendingHit = false;
        ApplyDamage();
    }

    private void ApplyDamage()
    {
        if (player == null || playerHealth == null || playerHealth.EstaMuerto) return;

        // Si el Player se alejó antes del golpe, el golpe falla.
        if (PlanarDistanceTo(player.position) > attackRange + hitRangeMargin) return;

        playerHealth.RecibirDano(attackDamage);
    }

    // --------------------------------------------------------------- VISIÓN

    private bool CanSeePlayer(bool chasing)
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 toTarget = target - eye;
        float distance = toTarget.magnitude;

        float range = chasing ? visionRange * chaseRangeMultiplier : visionRange;
        if (distance > range) return false;

        // Escondido bajo una mesa: solo te ve si está pegado a vos (hiddenDetectRange).
        if (playerScript != null && playerScript.IsHidden && distance > hiddenDetectRange) return false;

        // Fuera del cono de visión (salvo que esté muy cerca o ya lo esté persiguiendo).
        if (!chasing && distance > closeDetectRange)
        {
            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
            if (flat.sqrMagnitude > 0.0001f && Vector3.Angle(transform.forward, flat) > visionAngle * 0.5f)
                return false;
        }

        return HasLineOfSight(eye, toTarget, distance);
    }

    /// <summary>True si lo primero que tapa la vista hacia el Player es el propio Player (no una pared).</summary>
    private bool HasLineOfSight(Vector3 eye, Vector3 toTarget, float distance)
    {
        if (distance < 0.01f) return true;

        int count = Physics.RaycastNonAlloc(eye, toTarget / distance, hits, distance, obstacleMask,
                                            QueryTriggerInteraction.Ignore);

        float nearest = float.MaxValue;
        Transform nearestTransform = null;

        for (int i = 0; i < count; i++)
        {
            Transform t = hits[i].transform;
            if (t.IsChildOf(transform)) continue;           // ignorar al propio zombie (hacha, etc.)
            if (hits[i].distance < nearest)
            {
                nearest = hits[i].distance;
                nearestTransform = t;
            }
        }

        if (nearestTransform == null) return true;           // nada en el medio
        return nearestTransform == player || nearestTransform.IsChildOf(player);
    }

    // ------------------------------------------------------------- AUXILIARES

    private float PlanarDistanceTo(Vector3 point)
    {
        Vector3 d = point - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    private void FaceTarget(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        if (animator == null || !animHasSpeed) return;
        animator.SetFloat(SpeedHash, agent.isStopped ? 0f : agent.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    private void OnValidate()
    {
        closeDetectRange = Mathf.Min(closeDetectRange, visionRange);
        zoneRadius = Mathf.Max(0.5f, zoneRadius);
        waitTime.y = Mathf.Max(waitTime.x, waitTime.y);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = zoneCenter != null ? zoneCenter.position
                       : (Application.isPlaying ? home : transform.position);

        // Zona de ronda
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, zoneRadius);

        // Visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Vector3 left = Quaternion.Euler(0f, -visionAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, visionAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + left * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + right * visionRange);

        // Ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

//using UnityEngine;
//using UnityEngine.AI;


//[RequireComponent(typeof(NavMeshAgent))]
//public class ZombieAI : MonoBehaviour
//{
//    private enum State { Patrol, Chase }

//    [Header("Referencias (opcionales)")]
//    [Tooltip("Si queda vacío, busca solo al objeto que tenga PlayerNew.")]
//    [SerializeField] private Transform player;
//    [Tooltip("Si queda vacío, lo busca en los hijos.")]
//    [SerializeField] private Animator animator;

//    [Header("Zona de ronda")]
//    [Tooltip("Centro de la zona. Si queda vacío, usa el punto donde está el zombie al empezar.")]
//    [SerializeField] private Transform zoneCenter;
//    [SerializeField] private float zoneRadius = 8f;
//    [Tooltip("Segundos que se queda quieto al llegar a cada punto (mínimo, máximo).")]
//    [SerializeField] private Vector2 waitTime = new Vector2(2f, 5f);
//    [SerializeField] private float patrolSpeed = 1f;

//    [Header("Visión")]
//    [SerializeField] private float visionRange = 10f;
//    [Tooltip("Ángulo total del cono de visión, hacia adelante.")]
//    [SerializeField, Range(10f, 360f)] private float visionAngle = 120f;
//    [Tooltip("A esta distancia o menos te detecta aunque estés a sus espaldas.")]
//    [SerializeField] private float closeDetectRange = 2f;
//    [Tooltip("Altura de los ojos del zombie.")]
//    [SerializeField] private float eyeHeight = 1.6f;
//    [Tooltip("Altura del punto del Player al que apunta (el pecho).")]
//    [SerializeField] private float targetHeight = 1f;
//    [Tooltip("Qué capas bloquean la vista (paredes, puertas, muebles).")]
//    [SerializeField] private LayerMask obstacleMask = ~0;

//    [Header("Persecución")]
//    [SerializeField] private float chaseSpeed = 3.5f;
//    [Tooltip("Mientras persigue, te sigue viendo hasta rango x este valor (aunque estés de espaldas).")]
//    [SerializeField] private float chaseRangeMultiplier = 1.5f;
//    [Tooltip("Segundos que sigue buscando después de perderte de vista, antes de volver a rondar.")]
//    [SerializeField] private float loseSightTime = 4f;

//    [Header("Ataque")]
//    [SerializeField] private bool canAttack = true;
//    [SerializeField] private float attackRange = 1.6f;
//    [SerializeField] private float attackDamage = 34f;
//    [SerializeField] private float attackCooldown = 1.5f;
//    [Tooltip("Si está activo, el daño se aplica cuando la animación de ataque llama a OnAttackHit() " +
//             "(Animation Event en el frame del golpe). Si está apagado, se aplica apenas arranca el ataque.")]
//    [SerializeField] private bool damageOnAnimationEvent = true;
//    [Tooltip("Margen extra de distancia en el momento del golpe. Si te alejaste más que Attack Range + este valor, el golpe falla.")]
//    [SerializeField] private float hitRangeMargin = 0.5f;

//    // Parámetros del Animator (opcionales: si no existen, simplemente no se usan).
//    private static readonly int SpeedHash = Animator.StringToHash("Speed");
//    private static readonly int AttackHash = Animator.StringToHash("Attack");

//    private NavMeshAgent agent;
//    private PlayerHealth playerHealth;
//    private State state = State.Patrol;
//    private Vector3 home;
//    private bool animHasSpeed;
//    private bool animHasAttack;

//    private bool waiting;
//    private float waitTimer;
//    private float lostTimer;
//    private float repathTimer;
//    private float nextAttackTime;
//    private bool pendingHit;
//    private Vector3 lastKnownPlayerPos;

//    private readonly RaycastHit[] hits = new RaycastHit[16];
//    private const float PatrolStoppingDistance = 0.3f;

//    private void Awake()
//    {
//        agent = GetComponent<NavMeshAgent>();

//        if (animator == null) animator = GetComponentInChildren<Animator>();
//        if (animator != null)
//        {
//            // El movimiento lo hace el NavMeshAgent, no las animaciones.
//            animator.applyRootMotion = false;
//        }

//        if (animator != null && animator.runtimeAnimatorController != null)
//        {
//            foreach (AnimatorControllerParameter p in animator.parameters)
//            {
//                if (p.nameHash == SpeedHash && p.type == AnimatorControllerParameterType.Float) animHasSpeed = true;
//                if (p.nameHash == AttackHash && p.type == AnimatorControllerParameterType.Trigger) animHasAttack = true;
//            }
//        }
//    }

//    private void Start()
//    {
//        home = zoneCenter != null ? zoneCenter.position : transform.position;

//        if (player == null)
//        {
//            PlayerNew p = FindFirstObjectByType<PlayerNew>();
//            if (p != null) player = p.transform;
//        }

//        if (player != null) playerHealth = player.GetComponentInParent<PlayerHealth>();
//        else Debug.LogWarning("[ZombieAI] No encontré al Player en la escena.", this);

//        agent.speed = patrolSpeed;
//        agent.stoppingDistance = PatrolStoppingDistance;

//        if (!agent.isOnNavMesh)
//        {
//            Debug.LogError("[ZombieAI] El zombie no está sobre un NavMesh. Horneá el NavMesh (NavMeshSurface > Bake) " +
//                           "y revisá que el zombie esté apoyado en el piso.", this);
//            enabled = false;
//            return;
//        }

//        PickPatrolPoint();
//    }

//    private void Update()
//    {
//        if (player == null) return;

//        // Si el Player murió, el zombie se frena.
//        if (playerHealth != null && playerHealth.EstaMuerto)
//        {
//            agent.isStopped = true;
//            UpdateAnimator();
//            return;
//        }

//        switch (state)
//        {
//            case State.Patrol: UpdatePatrol(); break;
//            case State.Chase: UpdateChase(); break;
//        }

//        UpdateAnimator();
//    }

//    // ---------------------------------------------------------------- RONDA

//    private void UpdatePatrol()
//    {
//        if (CanSeePlayer(false))
//        {
//            StartChase();
//            return;
//        }

//        if (agent.pathPending) return;

//        bool arrived = !agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f;
//        if (!arrived) return;

//        if (!waiting)
//        {
//            waiting = true;
//            waitTimer = Random.Range(waitTime.x, waitTime.y);
//            return;
//        }

//        waitTimer -= Time.deltaTime;
//        if (waitTimer <= 0f)
//        {
//            waiting = false;
//            PickPatrolPoint();
//        }
//    }

//    /// <summary>Elige un punto al azar dentro de la zona al que el zombie realmente pueda llegar.</summary>
//    private void PickPatrolPoint()
//    {
//        Vector3 center = zoneCenter != null ? zoneCenter.position : home;
//        NavMeshPath path = new NavMeshPath();

//        for (int i = 0; i < 10; i++)
//        {
//            Vector2 offset = Random.insideUnitCircle * zoneRadius;
//            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

//            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 2f, NavMesh.AllAreas)) continue;
//            if (!agent.CalculatePath(navHit.position, path)) continue;
//            if (path.status != NavMeshPathStatus.PathComplete) continue;

//            agent.SetPath(path);
//            return;
//        }
//        // Si no encontró ninguno, reintenta después de la próxima espera.
//    }

//    // ---------------------------------------------------------- PERSECUCIÓN

//    private void StartChase()
//    {
//        state = State.Chase;
//        waiting = false;
//        lostTimer = 0f;
//        repathTimer = 0f;
//        lastKnownPlayerPos = player.position;

//        agent.isStopped = false;
//        agent.speed = chaseSpeed;
//        agent.stoppingDistance = canAttack ? Mathf.Max(PatrolStoppingDistance, attackRange * 0.8f) : PatrolStoppingDistance;
//    }

//    private void StopChase()
//    {
//        state = State.Patrol;
//        waiting = false;

//        agent.isStopped = false;
//        agent.speed = patrolSpeed;
//        agent.stoppingDistance = PatrolStoppingDistance;

//        PickPatrolPoint();
//    }

//    private void UpdateChase()
//    {
//        bool sees = CanSeePlayer(true);

//        if (sees)
//        {
//            lostTimer = 0f;
//            lastKnownPlayerPos = player.position;
//        }
//        else
//        {
//            lostTimer += Time.deltaTime;
//            if (lostTimer >= loseSightTime)
//            {
//                StopChase();
//                return;
//            }
//        }

//        // Ataque cuerpo a cuerpo
//        if (canAttack && sees && PlanarDistanceTo(player.position) <= attackRange)
//        {
//            agent.isStopped = true;
//            FaceTarget(player.position);
//            TryAttack();
//            return;
//        }

//        agent.isStopped = false;
//        repathTimer -= Time.deltaTime;
//        if (repathTimer <= 0f)
//        {
//            repathTimer = 0.15f;
//            agent.SetDestination(sees ? player.position : lastKnownPlayerPos);
//        }
//    }

//    private void TryAttack()
//    {
//        if (Time.time < nextAttackTime) return;
//        nextAttackTime = Time.time + attackCooldown;

//        bool hasAttackAnim = animator != null && animHasAttack;
//        if (hasAttackAnim) animator.SetTrigger(AttackHash);

//        if (damageOnAnimationEvent && hasAttackAnim)
//            pendingHit = true;      // el daño lo aplica OnAttackHit(), en el frame del golpe
//        else
//            ApplyDamage();          // sin animación de ataque: daño inmediato
//    }

//    /// <summary>
//    /// La llama el Animation Event del clip de ataque, en el frame en que el brazo golpea.
//    /// Tiene que ser public, sin parámetros, y el script tiene que estar en el mismo objeto que el Animator.
//    /// </summary>
//    public void OnAttackHit()
//    {
//        if (!pendingHit) return;    // evita golpes dobles o eventos sueltos
//        pendingHit = false;
//        ApplyDamage();
//    }

//    private void ApplyDamage()
//    {
//        if (player == null || playerHealth == null || playerHealth.EstaMuerto) return;

//        // Si el Player se alejó antes del golpe, el golpe falla.
//        if (PlanarDistanceTo(player.position) > attackRange + hitRangeMargin) return;

//        playerHealth.RecibirDano(attackDamage);
//    }

//    // --------------------------------------------------------------- VISIÓN

//    private bool CanSeePlayer(bool chasing)
//    {
//        Vector3 eye = transform.position + Vector3.up * eyeHeight;
//        Vector3 target = player.position + Vector3.up * targetHeight;
//        Vector3 toTarget = target - eye;
//        float distance = toTarget.magnitude;

//        float range = chasing ? visionRange * chaseRangeMultiplier : visionRange;
//        if (distance > range) return false;

//        // Fuera del cono de visión (salvo que esté muy cerca o ya lo esté persiguiendo).
//        if (!chasing && distance > closeDetectRange)
//        {
//            Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
//            if (flat.sqrMagnitude > 0.0001f && Vector3.Angle(transform.forward, flat) > visionAngle * 0.5f)
//                return false;
//        }

//        return HasLineOfSight(eye, toTarget, distance);
//    }

//    /// <summary>True si lo primero que tapa la vista hacia el Player es el propio Player (no una pared).</summary>
//    private bool HasLineOfSight(Vector3 eye, Vector3 toTarget, float distance)
//    {
//        if (distance < 0.01f) return true;

//        int count = Physics.RaycastNonAlloc(eye, toTarget / distance, hits, distance, obstacleMask,
//                                            QueryTriggerInteraction.Ignore);

//        float nearest = float.MaxValue;
//        Transform nearestTransform = null;

//        for (int i = 0; i < count; i++)
//        {
//            Transform t = hits[i].transform;
//            if (t.IsChildOf(transform)) continue;           // ignorar al propio zombie (hacha, etc.)
//            if (hits[i].distance < nearest)
//            {
//                nearest = hits[i].distance;
//                nearestTransform = t;
//            }
//        }

//        if (nearestTransform == null) return true;           // nada en el medio
//        return nearestTransform == player || nearestTransform.IsChildOf(player);
//    }

//    // ------------------------------------------------------------- AUXILIARES

//    private float PlanarDistanceTo(Vector3 point)
//    {
//        Vector3 d = point - transform.position;
//        d.y = 0f;
//        return d.magnitude;
//    }

//    private void FaceTarget(Vector3 point)
//    {
//        Vector3 dir = point - transform.position;
//        dir.y = 0f;
//        if (dir.sqrMagnitude < 0.0001f) return;

//        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
//    }

//    private void UpdateAnimator()
//    {
//        if (animator == null || !animHasSpeed) return;
//        animator.SetFloat(SpeedHash, agent.isStopped ? 0f : agent.velocity.magnitude, 0.1f, Time.deltaTime);
//    }

//    private void OnValidate()
//    {
//        closeDetectRange = Mathf.Min(closeDetectRange, visionRange);
//        zoneRadius = Mathf.Max(0.5f, zoneRadius);
//        waitTime.y = Mathf.Max(waitTime.x, waitTime.y);
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Vector3 center = zoneCenter != null ? zoneCenter.position
//                       : (Application.isPlaying ? home : transform.position);

//        // Zona de ronda
//        Gizmos.color = Color.green;
//        Gizmos.DrawWireSphere(center, zoneRadius);

//        // Visión
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, visionRange);
//        Vector3 left = Quaternion.Euler(0f, -visionAngle * 0.5f, 0f) * transform.forward;
//        Vector3 right = Quaternion.Euler(0f, visionAngle * 0.5f, 0f) * transform.forward;
//        Gizmos.DrawLine(transform.position, transform.position + left * visionRange);
//        Gizmos.DrawLine(transform.position, transform.position + right * visionRange);

//        // Ataque
//        Gizmos.color = Color.red;
//        Gizmos.DrawWireSphere(transform.position, attackRange);
//    }
//}