using UnityEngine;

/// <summary>
/// Trigger que frena al enemigo: cuando el Player entra, el enemigo deja de perseguir y se queda quieto.
/// Poner en un objeto vacío con un Collider (Is Trigger activado), antes de la puerta.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ChaseStopTrigger : MonoBehaviour
{
    [Tooltip("El Zombie (instancia de la escena) que se tiene que frenar.")]
    [SerializeField] private EnemyPatrol enemigo;
    [Tooltip("Si está activo, el trigger se destruye después de usarse.")]
    [SerializeField] private bool destruirAlUsar = true;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (enemigo != null) Destroy(enemigo.gameObject, 1.5f);
        //if (enemigo != null) enemigo.DetenerPersecucion();
        else Debug.LogWarning("[ChaseStopTrigger] Falta asignar el Enemigo.", this);

        if (destruirAlUsar) Destroy(gameObject);
    }
}