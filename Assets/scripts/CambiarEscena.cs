using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cuando el Player entra al trigger, carga otra escena (por ejemplo "Final").
/// Poner en un objeto vacío con un Collider (Is Trigger activado).
/// </summary>
[RequireComponent(typeof(Collider))]
public class CambiarEscena: MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar. Tiene que estar en Build Profiles > Scene List.")]
    [SerializeField] private string nombreEscena = "Final";

    private bool yaActivado;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaActivado || !other.CompareTag("Player")) return;

        if (!Application.CanStreamedLevelBeLoaded(nombreEscena))
        {
            Debug.LogError("[CambiarEscenaTrigger] No encuentro la escena '" + nombreEscena + "'. " +
                           "Revisá el nombre y que esté en File > Build Profiles > Scene List.", this);
            return;
        }

        yaActivado = true;
        SceneManager.LoadScene(nombreEscena);
    }
}