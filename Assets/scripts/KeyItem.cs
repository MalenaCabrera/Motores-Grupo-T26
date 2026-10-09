using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Configuración de la Llave")]
    [Tooltip("Nombre o ID de la llave. Debe coincidir con el que pide la puerta.")]
    public string keyID = "LlavePrincipal";

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip pickupSound;

    public void Recoger(PlayerNew player)
    {
        player.AgregarLlave(keyID);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        Destroy(gameObject);
    }
}
