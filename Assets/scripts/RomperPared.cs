using UnityEngine;

public class RomperPared : MonoBehaviour
{
    [Header("Configuración del Agujero")]
    [SerializeField] private EnemyPatrol enemigo;
    [SerializeField] private GameObject paredARomper;

    private void OnTriggerEnter(Collider other)
    {
        // Detecta si el jugador cruza la zona
        if (other.CompareTag("Player"))
        {
            
            if (paredARomper != null)
            {
                Destroy(paredARomper); 
                // se podria agregar un efecto de de la pared rompiendose a futuro
            }

            // si Enemigo no es null, comienza la persecución
            if (enemigo != null)
            {
                enemigo.IniciarPersecucion(other.transform);
            }

            // destruye el objeto trigger
            Destroy(gameObject);
        }
    }
}
