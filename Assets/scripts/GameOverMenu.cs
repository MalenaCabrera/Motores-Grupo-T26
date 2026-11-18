using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Botones de las pantallas de Game Over (sirve para las dos escenas).
/// Ponerlo en un objeto vacío (por ejemplo "GameOverManager") y conectar los botones desde OnClick.
/// </summary>
public class GameOverMenu : MonoBehaviour
{
    [Header("Escenas")]
    [Tooltip("Escena que se carga al tocar Reiniciar.")]
    [SerializeField] private string levelSceneName = "Nivel 1";
    [Tooltip("Escena que se carga al tocar Volver al Menu.")]
    [SerializeField] private string menuSceneName = "Menu";

    private void Start()
    {
        // El Player dejó el mouse bloqueado y oculto: lo liberamos para poder tocar los botones.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    /// <summary>Botón Reiniciar: vuelve a cargar el nivel desde cero.</summary>
    public void Reiniciar()
    {
        CargarEscena(levelSceneName);
    }

    /// <summary>Botón Volver al Menu.</summary>
    public void VolverAlMenu()
    {
        CargarEscena(menuSceneName);
    }

    private void CargarEscena(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("[GameOverMenu] No encuentro la escena '" + sceneName + "'. " +
                           "Revisá el nombre y que esté en File > Build Profiles > Scene List.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}