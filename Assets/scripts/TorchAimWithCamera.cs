using UnityEngine;

/// <summary>
/// Hace que la luz de la linterna apunte siempre hacia donde mira la cámara,
/// aunque la mano del personaje apunte a otro lado.
/// Poner en el mismo objeto que tiene el componente Light.
/// </summary>
[RequireComponent(typeof(Light))]
public class TorchAimWithCamera : MonoBehaviour
{
    [Tooltip("Si queda vacío, usa la Main Camera.")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Ajuste fino en grados, por si el haz sale levemente torcido.")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            Debug.LogWarning("TorchAimWithCamera: no encontré la cámara.", this);
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        transform.rotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
    }
}