using UnityEngine;

public class TitileoLuz : MonoBehaviour
{
    public Light luz;
    public float intensidadMin = 0.7f;
    public float intensidadMax = 1.3f;
    public float velocidad = 8f;

    private float semilla;

    void Start()
    {
        if (luz == null) luz = GetComponent<Light>();
        semilla = Random.Range(0f, 100f);
    }

    void Update()
    {
        float ruido = Mathf.PerlinNoise(Time.time * velocidad, semilla);
        luz.intensity = Mathf.Lerp(intensidadMin, intensidadMax, ruido);
    }
}