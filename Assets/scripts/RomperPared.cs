using UnityEngine;

public class RomperPared : MonoBehaviour
{
    [Header("Configuracionn de la Persecucion")]
    [SerializeField] private EnemyPatrol enemigo;
    [SerializeField] private GameObject paredARomper;

    private void OnTriggerEnter(Collider other)
    {
        // Detects if the object that entered has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Si el enemigo no es null, hace el llamado
            if (enemigo != null)
            {
                //ssss
                enemigo.IniciarPersecucion(other.transform, paredARomper);
            }

            // Destroy the trigger that collides with the player
            Destroy(gameObject);
        }
    }
}
