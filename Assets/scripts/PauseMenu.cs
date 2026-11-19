using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menú de pausa: Reanudar, Volver al menú y volumen general.
/// Se abre y se cierra con Escape. Poner en un objeto vacío del Nivel 1 (por ejemplo "PauseManager").
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private const string VolumeKey = "VolumenGeneral";

    [Header("UI")]
    [Tooltip("Panel con los botones y el slider. Se oculta solo al empezar.")]
    [SerializeField] private GameObject pausePanel;
    [Tooltip("Slider de volumen: Min 0, Max 1.")]
    [SerializeField] private Slider volumeSlider;
    [Tooltip("Opcional: texto que muestra el porcentaje (por ejemplo \"80%\").")]
    [SerializeField] private TMP_Text volumeText;

    [Header("Escenas")]
    [SerializeField] private string menuSceneName = "Menu";

    private PlayerNew player;
    private PlayerHealth playerHealth;
    private bool paused;
    private bool playerWasEnabled;

    public bool IsPaused => paused;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerNew>();
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        // Recupera el volumen guardado (1 = máximo la primera vez).
        float saved = PlayerPrefs.GetFloat(VolumeKey, 1f);
        ApplyVolume(saved);

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(saved);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (paused) Reanudar();
        else Pausar();
    }

    /// <summary>Abre el menú de pausa y congela el juego.</summary>
    public void Pausar()
    {
        // No se puede pausar si el Player ya murió.
        if (paused || (playerHealth != null && playerHealth.EstaMuerto)) return;

        paused = true;
        Time.timeScale = 0f;

        // Congela los controles del Player (cámara y movimiento).
        if (player != null)
        {
            playerWasEnabled = player.enabled;
            player.enabled = false;
        }

        if (pausePanel != null) pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Botón Reanudar (también Escape).</summary>
    public void Reanudar()
    {
        if (!paused) return;

        paused = false;
        Time.timeScale = 1f;

        if (player != null) player.enabled = playerWasEnabled;
        if (pausePanel != null) pausePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Botón Volver al menú.</summary>
    public void VolverAlMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            Debug.LogError("[PauseMenu] No encuentro la escena '" + menuSceneName + "'. " +
                           "Revisá el nombre y que esté en File > Build Profiles > Scene List.", this);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>Slider de volumen (0 a 1). Se guarda para la próxima vez.</summary>
    public void SetVolume(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(VolumeKey, value);
    }

    private void ApplyVolume(float value)
    {
        value = Mathf.Clamp01(value);
        AudioListener.volume = value;
        if (volumeText != null) volumeText.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    private void OnDestroy()
    {
        // Por si se cambia de escena estando en pausa.
        Time.timeScale = 1f;
    }
}
