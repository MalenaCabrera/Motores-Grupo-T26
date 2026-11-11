using UnityEngine;
using UnityEngine.UI;


public class VidaHUD : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Si queda vacío, busca solo al objeto que tenga PlayerHealth.")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image barraRelleno;
    [Tooltip("Opcional: imagen roja a pantalla completa que destella al recibir daño.")]
    [SerializeField] private Image flashDano;

    [Header("Barra")]
    [Tooltip("Qué tan rápido se mueve la barra hacia el valor real.")]
    [SerializeField] private float velocidadBarra = 6f;
    [SerializeField] private Color colorVidaLlena = new Color(0.25f, 0.8f, 0.3f, 1f);
    [SerializeField] private Color colorVidaBaja = new Color(0.85f, 0.15f, 0.15f, 1f);

    [Header("Destello de daño")]
    [SerializeField, Range(0f, 1f)] private float intensidadFlash = 0.45f;
    [Tooltip("Segundos que tarda el destello en desaparecer.")]
    [SerializeField] private float duracionFlash = 0.5f;

    private float fillActual = 1f;
    private float vidaAnterior;
    private float flashTimer;

    private void Start()
    {
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("[VidaHUD] No encontré ningún PlayerHealth en la escena.", this);
            enabled = false;
            return;
        }

        vidaAnterior = playerHealth.VidaActual;
        fillActual = Ratio();
        AplicarBarra();
        AplicarFlash(0f);
    }

    private void Update()
    {
        
        if (playerHealth.VidaActual < vidaAnterior) flashTimer = duracionFlash;
        vidaAnterior = playerHealth.VidaActual;

        
        fillActual = Mathf.MoveTowards(fillActual, Ratio(), velocidadBarra * Time.unscaledDeltaTime);
        AplicarBarra();

        
        if (flashTimer > 0f) flashTimer -= Time.unscaledDeltaTime;
        AplicarFlash(duracionFlash > 0f ? Mathf.Clamp01(flashTimer / duracionFlash) : 0f);
    }

    private float Ratio()
    {
        return playerHealth.VidaMaxima > 0f ? Mathf.Clamp01(playerHealth.VidaActual / playerHealth.VidaMaxima) : 0f;
    }

    private void AplicarBarra()
    {
        if (barraRelleno == null) return;
        barraRelleno.fillAmount = fillActual;
        barraRelleno.color = Color.Lerp(colorVidaBaja, colorVidaLlena, fillActual);
    }

    private void AplicarFlash(float t)
    {
        if (flashDano == null) return;
        Color c = flashDano.color;
        c.a = intensidadFlash * t;
        flashDano.color = c;
    }
}