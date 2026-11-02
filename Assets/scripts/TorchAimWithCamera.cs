using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchAimWithCamera : MonoBehaviour
{
    [Tooltip("Si queda vacío, usa la Main Camera.")]
    [SerializeField] private Transform cameraTransform;

    [Tooltip("Ajuste fino en grados, por si el haz sale levemente torcido.")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Tooltip("Máximo ángulo horizontal que puede separarse la luz del frente del cuerpo. " +
             "Tiene que ser MAYOR que 'Torch Turn Threshold' del PlayerNew.")]
    [SerializeField] private float maxAngleFromBody = 80f;

    private Transform body;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        PlayerNew player = GetComponentInParent<PlayerNew>();
        if (player != null) body = player.transform;

        if (cameraTransform == null)
            Debug.LogWarning("TorchAimWithCamera: no encontré la cámara.", this);
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        Quaternion rot = cameraTransform.rotation * Quaternion.Euler(rotationOffset);

        if (body != null)
        {
            Vector3 flat = Vector3.ProjectOnPlane(rot * Vector3.forward, Vector3.up);
            if (flat.sqrMagnitude > 0.0001f)
            {
                float yawDiff = Vector3.SignedAngle(body.forward, flat, Vector3.up);
                float clamped = Mathf.Clamp(yawDiff, -maxAngleFromBody, maxAngleFromBody);
                if (!Mathf.Approximately(clamped, yawDiff))
                    rot = Quaternion.AngleAxis(clamped - yawDiff, Vector3.up) * rot;
            }
        }

        transform.rotation = rot;
    }
}


//using UnityEngine;

///// <summary>
///// Hace que la luz de la linterna apunte siempre hacia donde mira la cámara,
///// aunque la mano del personaje apunte a otro lado.
///// Poner en el mismo objeto que tiene el componente Light.
///// </summary>
//[RequireComponent(typeof(Light))]
//public class TorchAimWithCamera : MonoBehaviour
//{
//    [Tooltip("Si queda vacío, usa la Main Camera.")]
//    [SerializeField] private Transform cameraTransform;

//    [Tooltip("Ajuste fino en grados, por si el haz sale levemente torcido.")]
//    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

//    private void Start()
//    {
//        if (cameraTransform == null && Camera.main != null)
//        {
//            cameraTransform = Camera.main.transform;
//        }

//        if (cameraTransform == null)
//        {
//            Debug.LogWarning("TorchAimWithCamera: no encontré la cámara.", this);
//        }
//    }

//    private void LateUpdate()
//    {
//        if (cameraTransform == null) return;

//        transform.rotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
//    }
//}