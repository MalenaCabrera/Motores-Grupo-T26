using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menú principal: botón Jugar, botón Créditos (con su panel) y botón Salir.
/// Ponerlo en un objeto vacío de la escena del menú (por ejemplo "MenuManager")
/// y conectar los botones desde su evento OnClick.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego")]
    [Tooltip("Nombre exacto de la escena que se carga al tocar Jugar. Tiene que estar en Build Settings.")]
    [SerializeField] private string gameSceneName = "Nivel 1";

    [Tooltip("Escena a la que vuelve el botón Volver al menú.")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Paneles")]
    [Tooltip("Panel con el título y los botones principales.")]
    [SerializeField] private GameObject mainPanel;
    [Tooltip("Panel con los créditos y el botón Volver.")]
    [SerializeField] private GameObject creditsPanel;

    private void Start()
    {
        // Si volvés de una partida, el Player dejó el mouse bloqueado y el tiempo puede estar pausado.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        CerrarCreditos();
    }

    /// <summary>Botón Jugar: carga la escena del juego.</summary>
    public void Jugar()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError("[MainMenu] No encuentro la escena '" + gameSceneName + "'. " +
                           "Revisá que el nombre sea exacto y que esté en File > Build Profiles > Scene List.", this);
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>Botón Volver al menú (para la escena de victoria).</summary>
    public void VolverAlMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            Debug.LogError("[MainMenu] No encuentro la escena '" + menuSceneName + "'. " +
                           "Revisá que el nombre sea exacto y que esté en File > Build Profiles > Scene List.", this);
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>Botón Créditos: muestra el panel de créditos.</summary>
    public void AbrirCreditos()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    /// <summary>Botón Volver (dentro de los créditos): vuelve al menú principal.</summary>
    public void CerrarCreditos()
    {
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    /// <summary>Botón Salir: cierra el juego (en el editor, detiene el Play).</summary>
    public void Salir()
    {
        Debug.Log("[MainMenu] Saliendo...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}