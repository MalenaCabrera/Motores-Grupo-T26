using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    //Setteable max health
    [Header("Vida del Player")]
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaActual;

    //After taking damage, the player cant be hurt again until few seconds
    [Header("Invulnerabilidad luego de recibir daño")]
    [SerializeField] private float tiempoInvulnerabilidad = 0.5f;
    private bool esInvulnerable;

    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;
    public bool EstaMuerto => vidaActual <= 0f;

    void Awake() //se ejecuta antes que start (recodatorio para mas adelante)
    {
        //The player always starts at full health
        vidaActual = vidaMaxima;
    }

    public void RecibirDano(float cantidad)
    {
        //Ignore damage while invulnerable or already dead
        if (esInvulnerable || EstaMuerto) return;

        vidaActual = Mathf.Max(vidaActual - cantidad, 0f);

        Debug.Log($"[PlayerHealth] Recibió {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");

        if (EstaMuerto)
        {
            Morir();
        }
        else if (tiempoInvulnerabilidad > 0f)
        {
            StartCoroutine(InvulnerabilidadTemporal());
        }
    }

    public void Matar()
    {
        if (EstaMuerto) return;
        vidaActual = 0f;
        Morir();
    }

    private IEnumerator InvulnerabilidadTemporal()
    {
        esInvulnerable = true;
        yield return new WaitForSeconds(tiempoInvulnerabilidad);
        esInvulnerable = false;
    }

    private void Morir()
    {
        Debug.Log("[PlayerHealth] El jugador ha muerto.");
        //death animation, restart level, Game Over screen, etc..
    }
}