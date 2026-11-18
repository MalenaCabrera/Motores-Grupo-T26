



using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    //Cada enemigo le dice al morir qué pantalla de Game Over mostrar
    [Header("Game Over")]
    [Tooltip("Escena que se carga si quien mata al jugador no indica una propia.")]
    [SerializeField] private string escenaGameOverPorDefecto = "GameOverZombie1";
    [Tooltip("Segundos entre la muerte y el cambio de escena.")]
    [SerializeField] private float retrasoGameOver = 1.5f;
    private string escenaGameOver;      // la define quien da el golpe final
    private bool gameOverEnCurso;

    public float VidaActual => vidaActual;
    public float VidaMaxima => vidaMaxima;
    public bool EstaMuerto => vidaActual <= 0f;

    void Awake() //se ejecuta antes que start (recodatorio para mas adelante)
    {
        //The player always starts at full health
        vidaActual = vidaMaxima;
    }

    //escenaGameOverDelAtacante es opcional: si no se pasa, se usa escenaGameOverPorDefecto
    public void RecibirDano(float cantidad, string escenaGameOverDelAtacante = null)
    {
        //Ignore damage while invulnerable or already dead
        if (esInvulnerable || EstaMuerto) return;

        vidaActual = Mathf.Max(vidaActual - cantidad, 0f);

        Debug.Log($"[PlayerHealth] Recibió {cantidad} de daño. Vida: {vidaActual}/{vidaMaxima}");

        if (EstaMuerto)
        {
            escenaGameOver = escenaGameOverDelAtacante;
            Morir();
        }
        else if (tiempoInvulnerabilidad > 0f)
        {
            StartCoroutine(InvulnerabilidadTemporal());
        }
    }

    public void Matar(string escenaGameOverDelAtacante = null)
    {
        if (EstaMuerto) return;
        vidaActual = 0f;
        escenaGameOver = escenaGameOverDelAtacante;
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

        if (gameOverEnCurso) return;
        gameOverEnCurso = true;


        //Reproduce la animación de muerte 
        PlayerAnimation animacion = GetComponent<PlayerAnimation>();
        if (animacion != null) animacion.PlayDeath();


        //Congela los controles mientras se espera el cambio de escena
        PlayerNew controles = GetComponent<PlayerNew>();
        if (controles != null) controles.enabled = false;

        StartCoroutine(CargarGameOver());
    }

    private IEnumerator CargarGameOver()
    {
        yield return new WaitForSecondsRealtime(retrasoGameOver);

        string escena = string.IsNullOrEmpty(escenaGameOver) ? escenaGameOverPorDefecto : escenaGameOver;

        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            Debug.LogError("[PlayerHealth] No encuentro la escena '" + escena + "'. " +
                           "Revisá el nombre y que esté en File > Build Profiles > Scene List.", this);
            yield break;
        }

        SceneManager.LoadScene(escena);
    }
}