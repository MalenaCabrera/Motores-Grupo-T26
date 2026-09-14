using UnityEngine;

public class Script_Door : MonoBehaviour
{
    private bool isOpen = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 3f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        // Interpola suavemente hacia el objetivo
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
    }
}
