using UnityEngine;
using UnityEngine.Audio;

public class TriggerTunelCaida : MonoBehaviour
{
    [SerializeField] private AudioSource sonidoCaida;
    [SerializeField] private AudioMixerSnapshot snapshotBackrooms;
    [SerializeField] private float duracionTransicion = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Reproduce el sonido de caída al instante
            if (sonidoCaida != null)
            {
                sonidoCaida.Play();
            }

            // 2. Transiciona la música a los Backrooms
            if (snapshotBackrooms != null)
            {
                snapshotBackrooms.TransitionTo(duracionTransicion);
            }

            // 3. Desactiva el collider para que no se repita (evita destruir el objeto y cortar el audio)
            GetComponent<Collider>().enabled = false;
        }
    }
}