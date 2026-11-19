using UnityEngine;

/// <summary>
/// Cambia a la música de persecución cuando CUALQUIER enemigo (ZombieAI o EnemyPatrol) persigue al Player.
/// Cuando ninguno persigue, hace un fundido de vuelta a la música normal.
///
/// No toca los snapshots del Audio Mixer: solo sube/baja el volumen de los AudioSource,
/// así que convive con TriggerCaida y con cualquier otro cambio de snapshot.
/// Poner en un objeto vacío de la escena del nivel (por ejemplo "ChaseMusicManager").
/// </summary>
public class ChaseMusicManager : MonoBehaviour
{
    [Header("Música de persecución")]
    [Tooltip("AudioSource con la música de persecución: Loop activado y Play On Awake apagado.")]
    [SerializeField] private AudioSource chaseMusic;
    [SerializeField, Range(0f, 1f)] private float chaseVolume = 1f;

    [Header("Música normal (se baja durante la persecución)")]
    [Tooltip("Los AudioSource de la música del nivel (MusicaBackrooms, MusicaPanaderia...).")]
    [SerializeField] private AudioSource[] ambientMusic;

    [Header("Transición")]
    [Tooltip("Segundos en que entra la música de persecución.")]
    [SerializeField] private float fadeInTime = 1f;
    [Tooltip("Segundos en que vuelve la música normal.")]
    [SerializeField] private float fadeOutTime = 3f;
    [Tooltip("Segundos que se mantiene la música de persecución después de que el último enemigo deja de perseguir.")]
    [SerializeField] private float lingerTime = 4f;

    private float[] ambientBaseVolumes;
    private ZombieAI[] zombies = new ZombieAI[0];
    private EnemyPatrol[] patrols = new EnemyPatrol[0];
    private PlayerHealth playerHealth;

    private float chaseAmount;      // 0 = música normal, 1 = música de persecución
    private float lingerTimer;
    private float nextSearchTime;

    private void Start()
    {
        ambientBaseVolumes = new float[ambientMusic.Length];
        for (int i = 0; i < ambientMusic.Length; i++)
        {
            ambientBaseVolumes[i] = ambientMusic[i] != null ? ambientMusic[i].volume : 1f;
        }

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (chaseMusic != null)
        {
            chaseMusic.playOnAwake = false;
            chaseMusic.loop = true;
            chaseMusic.volume = 0f;
            chaseMusic.Stop();
        }
        else
        {
            Debug.LogWarning("[ChaseMusicManager] Falta asignar el AudioSource de 'Chase Music'.", this);
        }

        RefreshEnemies();
    }

    private void Update()
    {
        // Busca enemigos cada tanto, por si aparece alguno nuevo.
        if (Time.time >= nextSearchTime)
        {
            RefreshEnemies();
            nextSearchTime = Time.time + 2f;
        }

        if (AnyEnemyChasing()) lingerTimer = lingerTime;
        else lingerTimer -= Time.deltaTime;

        float target = lingerTimer > 0f ? 1f : 0f;
        float fadeTime = target > chaseAmount ? fadeInTime : fadeOutTime;
        chaseAmount = Mathf.MoveTowards(chaseAmount, target, Time.deltaTime / Mathf.Max(0.01f, fadeTime));

        ApplyVolumes();
    }

    private void RefreshEnemies()
    {
        zombies = FindObjectsByType<ZombieAI>(FindObjectsSortMode.None);
        patrols = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
    }

    private bool AnyEnemyChasing()
    {
        // Si el Player murió, ya no cuenta como persecución.
        if (playerHealth != null && playerHealth.EstaMuerto) return false;

        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] != null && zombies[i].IsChasing) return true;
        }

        for (int i = 0; i < patrols.Length; i++)
        {
            if (patrols[i] != null && patrols[i].IsChasing) return true;
        }

        return false;
    }

    private void ApplyVolumes()
    {
        for (int i = 0; i < ambientMusic.Length; i++)
        {
            if (ambientMusic[i] != null)
            {
                ambientMusic[i].volume = ambientBaseVolumes[i] * (1f - chaseAmount);
            }
        }

        if (chaseMusic == null) return;

        chaseMusic.volume = chaseAmount * chaseVolume;

        if (chaseAmount > 0f && !chaseMusic.isPlaying) chaseMusic.Play();
        else if (chaseAmount <= 0f && chaseMusic.isPlaying) chaseMusic.Stop();
    }
}