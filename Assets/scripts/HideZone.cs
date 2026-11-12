using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HideZone : MonoBehaviour
{
    private void Reset()
    {
        
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerNew player = other.GetComponentInParent<PlayerNew>();
        if (player != null) player.EnterHideZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerNew player = other.GetComponentInParent<PlayerNew>();
        if (player != null) player.ExitHideZone(this);
    }

    private void OnDisable()
    {
        
        PlayerNew player = FindFirstObjectByType<PlayerNew>();
        if (player != null) player.ExitHideZone(this);
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 0.6f, 1f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;
        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}