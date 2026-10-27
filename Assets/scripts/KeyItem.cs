using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Datos del Ítem")]
    [SerializeField] private Item itemData;

    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip pickupSound;

    public void Recoger(PlayerNew player)
    {
        if (player.Inventario == null)
        {
            Debug.LogError("KeyItem: el Player no tiene Inventory asignado.", this);
            return;
        }

        if (itemData == null)
        {
            Debug.LogError("KeyItem: falta asignar el Item (ScriptableObject) en el Inspector.", this);
            return;
        }

        if (!player.Inventario.AgregarItem(itemData)) return;

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        Destroy(gameObject);
    }

}

