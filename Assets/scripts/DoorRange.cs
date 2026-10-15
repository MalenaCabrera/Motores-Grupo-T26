using UnityEngine;

public class DoorRange : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 3f;

    [Header("Cerradura")]
    [SerializeField] private bool requiereLlave = false;
    [SerializeField] private string keyIDNecesaria = "LlavePrincipal";
    [SerializeField] private bool consumirLlaveAlAbrir = false;

    [Header("Sonidos (Opcional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;

    private bool isOpen = false;
    private bool isLocked;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        isLocked = requiereLlave;
    }

    void Update()
    {
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * speed
        );
    }

    public void Interactuar(PlayerNew player)
    {
        if (isLocked)
        {
            // Chequea si tenés la llave en la mano activa del inventario
            if (player.Inventario != null && player.Inventario.TieneItemEnMano(keyIDNecesaria))
            {
                isLocked = false;
                isOpen = true;
                Debug.Log("¡Puerta destrabada y abierta!");

                if (openSound != null)
                {
                    AudioSource.PlayClipAtPoint(openSound, transform.position);
                }

                if (consumirLlaveAlAbrir)
                {
                    player.Inventario.RemoverPorID(keyIDNecesaria);
                }
            }
            else
            {
                Debug.Log("Puerta trabada. Necesitás tener en la mano: " + keyIDNecesaria);
                if (lockedSound != null)
                {
                    AudioSource.PlayClipAtPoint(lockedSound, transform.position);
                }
            }
            return;
        }

        isOpen = !isOpen;
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }
}

//using UnityEngine;

//public class DoorRange : MonoBehaviour
//{
//    [Header("Movimiento")]
//    [SerializeField] private float openAngle = 90f;
//    [SerializeField] private float speed = 3f;

//    [Header("Cerradura")]
//    [SerializeField] private bool requiereLlave = false;
//    [SerializeField] private string keyIDNecesaria = "LlavePrincipal";
//    [SerializeField] private bool consumirLlaveAlAbrir = false;

//    [Header("Sonidos (Opcional)")]
//    [SerializeField] private AudioClip openSound;
//    [SerializeField] private AudioClip lockedSound;

//    private bool isOpen = false;
//    private bool isLocked;

//    private Quaternion closedRotation;
//    private Quaternion openRotation;

//    void Start()
//    {
//        closedRotation = transform.rotation;
//        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

//        isLocked = requiereLlave;
//    }

//    void Update()
//    {
//        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

//        transform.rotation = Quaternion.Slerp(
//            transform.rotation,
//            targetRotation,
//            Time.deltaTime * speed
//        );
//    }

//    public void Interactuar(PlayerNew player)
//    {
//        if (isLocked)
//        {
//            if (player.TieneLlave(keyIDNecesaria))
//            {
//                isLocked = false;
//                isOpen = true;
//                Debug.Log("¡Puerta destrabada y abierta!");

//                if (openSound != null)
//                {
//                    AudioSource.PlayClipAtPoint(openSound, transform.position);
//                }

//                if (consumirLlaveAlAbrir)
//                {
//                    player.RemoverLlave(keyIDNecesaria);
//                }
//            }
//            else
//            {
//                Debug.Log("Puerta trabada. Te falta la llave: " + keyIDNecesaria);
//                if (lockedSound != null)
//                {
//                    AudioSource.PlayClipAtPoint(lockedSound, transform.position);
//                }
//            }
//            return;
//        }

//        isOpen = !isOpen;
//        if (openSound != null)
//        {
//            AudioSource.PlayClipAtPoint(openSound, transform.position);
//        }
//    }
//}
