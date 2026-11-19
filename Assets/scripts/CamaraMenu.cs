using UnityEngine;

public class FlotarSuave : MonoBehaviour
{
    public float altura = 0.15f;
    public float velocidad = 0.7f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * velocidad) * altura;
        transform.position = posicionInicial + new Vector3(0f, y, 0f);
    }
}