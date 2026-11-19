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
            if (sonidoCaida != null)
            {
                sonidoCaida.Play();
            }

            if (snapshotBackrooms != null)
            {
                snapshotBackrooms.TransitionTo(duracionTransicion);
            }

            GetComponent<Collider>().enabled = false;
        }
    }
}