using UnityEngine;

/// <summary>
/// Muestra en la mano del personaje el ítem que está seleccionado en el inventario.
/// Poner en el GameObject del Player (el mismo que tiene Inventory).
/// </summary>
public class HeldItemDisplay : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Inventory inventory;

    [Tooltip("Objeto vacío que está dentro de la mano del personaje (hijo del hueso 'RightHand').")]
    [SerializeField] private Transform handSocket;

    [Header("Ajuste del objeto en la mano")]
    [SerializeField] private Vector3 heldPosition = Vector3.zero;
    [SerializeField] private Vector3 heldRotation = Vector3.zero;
    [SerializeField] private Vector3 heldScale = Vector3.one;

    private GameObject currentHeld;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponentInParent<Inventory>();
        }

        if (inventory == null)
        {
            Debug.LogError("HeldItemDisplay: no encontré el Inventory del jugador.", this);
        }

        if (handSocket == null)
        {
            Debug.LogError("HeldItemDisplay: falta asignar 'Hand Socket' (el objeto dentro de la mano).", this);
        }
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.OnSelectedItemChanged += Show;

        Debug.Log("[HeldItemDisplay] activo en '" + name + "' | inventory: " +
                  (inventory != null ? inventory.name : "NO ENCONTRADO") +
                  " | handSocket: " + (handSocket != null ? handSocket.name : "SIN ASIGNAR"), this);
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnSelectedItemChanged -= Show;
    }

    private void Show(Item item)
    {
        Debug.Log("[HeldItemDisplay] recibió: " + (item != null ? item.itemName : "mano vacía") +
                  " | handPrefab: " + (item != null && item.handPrefab != null ? item.handPrefab.name : "VACÍO"), this);

        // Sacar lo que había en la mano
        if (currentHeld != null)
        {
            Destroy(currentHeld);
            currentHeld = null;
        }

        if (item == null || item.handPrefab == null || handSocket == null) return;

        currentHeld = Instantiate(item.handPrefab, handSocket);
        currentHeld.transform.localPosition = heldPosition;
        currentHeld.transform.localRotation = Quaternion.Euler(heldRotation);
        currentHeld.transform.localScale = heldScale;

        // Lo que se lleva en la mano no tiene que chocar con el jugador ni con el mundo
        foreach (Collider c in currentHeld.GetComponentsInChildren<Collider>(true))
        {
            c.enabled = false;
        }

        foreach (Rigidbody rb in currentHeld.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
        }
    }
}



//using UnityEngine;

///// <summary>
///// Muestra en la mano del personaje el ítem que está seleccionado en el inventario.
///// Poner en el GameObject del Player (el mismo que tiene Inventory).
///// </summary>
//public class HeldItemDisplay : MonoBehaviour
//{
//    [Header("Referencias")]
//    [SerializeField] private Inventory inventory;

//    [Tooltip("Objeto vacío que está dentro de la mano del personaje (hijo del hueso 'RightHand').")]
//    [SerializeField] private Transform handSocket;

//    [Header("Ajuste del objeto en la mano")]
//    [SerializeField] private Vector3 heldPosition = Vector3.zero;
//    [SerializeField] private Vector3 heldRotation = Vector3.zero;
//    [SerializeField] private Vector3 heldScale = Vector3.one;

//    private GameObject currentHeld;

//    private void Awake()
//    {
//        if (inventory == null)
//        {
//            inventory = GetComponentInParent<Inventory>();
//        }

//        if (inventory == null)
//        {
//            Debug.LogError("HeldItemDisplay: no encontré el Inventory del jugador.", this);
//        }

//        if (handSocket == null)
//        {
//            Debug.LogError("HeldItemDisplay: falta asignar 'Hand Socket' (el objeto dentro de la mano).", this);
//        }
//    }

//    private void OnEnable()
//    {
//        if (inventory != null) inventory.OnSelectedItemChanged += Show;
//    }

//    private void OnDisable()
//    {
//        if (inventory != null) inventory.OnSelectedItemChanged -= Show;
//    }

//    private void Show(Item item)
//    {
//        // Sacar lo que había en la mano
//        if (currentHeld != null)
//        {
//            Destroy(currentHeld);
//            currentHeld = null;
//        }

//        if (item == null || item.handPrefab == null || handSocket == null) return;

//        currentHeld = Instantiate(item.handPrefab, handSocket);
//        currentHeld.transform.localPosition = heldPosition;
//        currentHeld.transform.localRotation = Quaternion.Euler(heldRotation);
//        currentHeld.transform.localScale = heldScale;

//        // Lo que se lleva en la mano no tiene que chocar con el jugador ni con el mundo
//        foreach (Collider c in currentHeld.GetComponentsInChildren<Collider>(true))
//        {
//            c.enabled = false;
//        }

//        foreach (Rigidbody rb in currentHeld.GetComponentsInChildren<Rigidbody>(true))
//        {
//            rb.isKinematic = true;
//        }
//    }
//}