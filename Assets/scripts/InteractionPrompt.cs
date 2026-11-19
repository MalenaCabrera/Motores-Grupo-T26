using UnityEngine;

/// <summary>
/// Muestra el cartel "E" (prefab EPrompt) sobre lo que el Player está mirando y puede usar:
/// puertas (DoorRange) y llaves / linterna (KeyItem).
/// Usa el mismo rayo que PlayerNew para interactuar, así que el cartel aparece exactamente
/// cuando apretar E funciona. No hay que tocar cada puerta ni cada objeto.
/// Poner en el GameObject del Player (o en un objeto vacío de la escena).
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("Cartel")]
    [Tooltip("El prefab EPrompt (Assets/prefabs/EPrompt.prefab).")]
    [SerializeField] private GameObject promptPrefab;
    [Tooltip("Qué tan separado de la superficie aparece el cartel, hacia la cámara (metros).")]
    [SerializeField] private float offsetTowardsCamera = 0.15f;
    [Tooltip("Sube el cartel un poco sobre el punto que mirás (metros).")]
    [SerializeField] private float heightOffset = 0.1f;

    [Header("Referencias (opcionales)")]
    [Tooltip("Si queda vacío se busca el PlayerNew de la escena.")]
    [SerializeField] private PlayerNew player;

    private GameObject prompt;
    private Transform cam;

    private void Start()
    {
        if (player == null) player = FindFirstObjectByType<PlayerNew>();

        if (promptPrefab == null)
        {
            Debug.LogWarning("[InteractionPrompt] Falta asignar el prefab EPrompt.", this);
            enabled = false;
            return;
        }

        prompt = Instantiate(promptPrefab);
        prompt.name = "EPrompt (runtime)";
        prompt.SetActive(false);
    }

    private void LateUpdate()
    {
        if (player == null || prompt == null) return;

        if (cam == null)
        {
            if (Camera.main != null) cam = Camera.main.transform;
            else return;
        }

        // Sin cartel mientras el juego está en pausa o el Player está congelado (nota abierta, muerte...).
        if (Time.timeScale == 0f || !player.enabled)
        {
            prompt.SetActive(false);
            return;
        }

        Ray ray = new Ray(cam.position, cam.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, player.interactionDistance, player.interactableLayer) &&
            EsInteractuable(hit.collider))
        {
            // Aparece donde mirás, un poco hacia la cámara para que no quede dentro de la puerta.
            prompt.transform.position = hit.point - ray.direction * offsetTowardsCamera + Vector3.up * heightOffset;
            prompt.transform.rotation = Quaternion.LookRotation(prompt.transform.position - cam.position);
            prompt.SetActive(true);
        }
        else
        {
            prompt.SetActive(false);
        }
    }

    private static bool EsInteractuable(Collider col)
    {
        return col.GetComponentInParent<DoorRange>() != null ||
               col.GetComponentInParent<KeyItem>() != null;
    }

    private void OnDisable()
    {
        if (prompt != null) prompt.SetActive(false);
    }
}