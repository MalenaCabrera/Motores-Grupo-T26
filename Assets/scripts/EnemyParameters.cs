using UnityEngine;

public class EnemyParameters : MonoBehaviour
{

    public float distanciaSeguir;
    public float distanciaAtacar;
    public float distanciaEscapar;
    void Start()
    {

    }

    void Update()
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaSeguir);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtacar);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, distanciaEscapar);

    }
}
