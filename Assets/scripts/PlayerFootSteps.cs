using UnityEngine;

/// <summary>
/// Sonidos de pasos del Player.
/// Poner en el GameObject "Player" (el mismo que tiene PlayerNew). Agrega solo un AudioSource.
/// El ritmo depende de la distancia recorrida, así que se adapta solo a caminar, correr y agacharse.
/// </summary>
[RequireComponent(typeof(PlayerNew))]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Sonidos")]
    [Tooltip("Varios sonidos de UN paso cada uno. En cada paso se elige uno al azar.")]
    [SerializeField] private AudioClip[] footstepClips;
    [Tooltip("Opcional: sonido al aterrizar de un salto. Si queda vacío se usa un paso normal.")]
    [SerializeField] private AudioClip landingClip;

    [Header("Ritmo: metros que recorre entre un paso y el siguiente")]
    [Tooltip("Si los pasos suenan muy seguidos, subí el valor. Si suenan muy espaciados, bajalo.")]
    [SerializeField] private float walkStepDistance = 1.8f;
    [SerializeField] private float sprintStepDistance = 2.4f;
    [SerializeField] private float crouchStepDistance = 1.2f;

    [Header("Volumen")]
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float sprintVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float crouchVolume = 0.25f;
    [Tooltip("Variación de tono al azar, para que los pasos no suenen siempre idéntico.")]
    [SerializeField] private Vector2 pitchRange = new Vector2(0.92f, 1.08f);

    [Header("Aterrizaje")]
    [Tooltip("Velocidad de caída (m/s) a partir de la cual suena el aterrizaje.")]
    [SerializeField] private float landingMinFallSpeed = 4f;

    private PlayerNew player;
    private AudioSource source;
    private float distanceSinceStep;
    private int lastClipIndex = -1;
    private bool wasGrounded = true;
    private float lowestVerticalVelocity;
    private bool warnedNoClips;

    private void Awake()
    {
        player = GetComponent<PlayerNew>();
        source = GetComponent<AudioSource>();

        // Sonido 2D, sin bucle y que no arranque solo.
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }

    private void Update()
    {
        // Si el Player está congelado (por ejemplo, murió), no hay pasos.
        if (!player.enabled) return;

        bool grounded = player.IsGrounded;
        HandleLanding(grounded);

        float speed = player.HorizontalSpeed;

        if (!grounded || speed < 0.1f)
        {
            // Quieto o en el aire: al empezar a moverse, el primer paso suena rápido.
            distanceSinceStep = CurrentStepDistance() * 0.5f;
            return;
        }

        distanceSinceStep += speed * Time.deltaTime;

        float stepDistance = CurrentStepDistance();
        if (distanceSinceStep >= stepDistance)
        {
            distanceSinceStep -= stepDistance;
            PlayClip(PickFootstep(), CurrentVolume());
        }
    }

    private void HandleLanding(bool grounded)
    {
        if (!grounded)
        {
            lowestVerticalVelocity = Mathf.Min(lowestVerticalVelocity, player.VerticalVelocity);
        }
        else if (!wasGrounded)
        {
            if (-lowestVerticalVelocity >= landingMinFallSpeed)
            {
                AudioClip clip = landingClip != null ? landingClip : PickFootstep();
                PlayClip(clip, sprintVolume);
            }

            lowestVerticalVelocity = 0f;
            distanceSinceStep = 0f;
        }

        wasGrounded = grounded;
    }

    private float CurrentStepDistance()
    {
        if (player.IsCrouching) return crouchStepDistance;
        if (player.IsSprinting) return sprintStepDistance;
        return walkStepDistance;
    }

    private float CurrentVolume()
    {
        if (player.IsCrouching) return crouchVolume;
        if (player.IsSprinting) return sprintVolume;
        return walkVolume;
    }

    private AudioClip PickFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
        {
            if (!warnedNoClips)
            {
                warnedNoClips = true;
                Debug.LogWarning("[PlayerFootsteps] No hay sonidos de pasos asignados en 'Footstep Clips'.", this);
            }
            return null;
        }

        // Elige uno al azar, evitando repetir el mismo dos veces seguidas.
        int index;
        do
        {
            index = Random.Range(0, footstepClips.Length);
        }
        while (footstepClips.Length > 1 && index == lastClipIndex);

        lastClipIndex = index;
        return footstepClips[index];
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null) return;

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, volume);
    }
}