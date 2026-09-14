using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Speed Between Waypoints")]
    [SerializeField] private float speed;

    //Here you set the Waypoints and "WaitTime" between Waypoints
    [Header("Waypoints/Rutines")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime;
    [SerializeField] private float rotationSpeed = 5.3f;
    private int currentWaypoint;
    private bool isWaiting;
    private bool isRotating;

    [Header("Persecución")]
    [SerializeField] private float velocidadPersecucion = 6f;
    private Transform targetJugador;
    private PlayerHealth vidaJugador; // reference to the health of the player we're chasing
    private bool estaPersiguiendo;

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[EnemyPatrol] {gameObject.name} no tiene Waypoints en el Inspector.");
            enabled = false;
            return;
        }
        Vector3 direction = (waypoints[0].position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void Update()
    {
        //this line is placed at the top to ensure that the pursuit is prioritized over routine
        if (estaPersiguiendo)
        {
            PerseguirAlJugador();
            return;
        }

        //Do not move while waiting or while turning towards the next Waypoint
        if (isWaiting || isRotating) return;

        //If the enemy hasnt reached the current waypoint, move towards it
        if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].position, speed * Time.deltaTime);
        }
        //This start the Coroutine between Waypoints if waiting
        else if (!isWaiting)
        {
            StartCoroutine(Wait());
        }
    }

    IEnumerator Wait()
    {
        isWaiting = true;
        //"waitTime" defines how long the enemy waits between each waypoint
        yield return new WaitForSeconds(waitTime);

        //Randomize nextWaypoint
        if (waypoints.Length == 1)
        {
            currentWaypoint = 0;
        }

        else
        {
            int nextWaypoint;
            do nextWaypoint = Random.Range(0, waypoints.Length);
            while (nextWaypoint == currentWaypoint);
            currentWaypoint = nextWaypoint;
        }
        isWaiting = false;
        StartCoroutine(RotBetweenWaypoints(waypoints[currentWaypoint].position));
    }

    //Smoothly rotates the enemy to face the next waypoint before it starts moving towards it
    IEnumerator RotBetweenWaypoints(Vector3 target)
    {
        isRotating = true;

        Vector3 direction = (target - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        isRotating = false;
    }
    public void IniciarPersecucion(Transform jugador, GameObject pared)
    {
        enabled = true;
        //stop all the coroutines
        StopAllCoroutines();
        isWaiting = false;
        isRotating = false;

        if (pared != null)
        {
            Destroy(pared);
            //(Aca se podrian agregar polvo o efectos mas adelante)
        }

        //Lock the target and start the chase
        targetJugador = jugador;
        vidaJugador = jugador.GetComponent<PlayerHealth>(); // look up the player's health once, when the chase starts
        estaPersiguiendo = true;
    }

    private void PerseguirAlJugador()
    {
        if (targetJugador == null) return;

        //If the player is already dead, stop moving and rotating entirely
        if (vidaJugador != null && vidaJugador.EstaMuerto)
        {
            return;
        }

        //this makes enemy to move in a straight line toward the player at running speed
        transform.position = Vector3.MoveTowards(transform.position, targetJugador.position, velocidadPersecucion * Time.deltaTime);

        //Rotate the plane on Y to face the player directly
        Vector3 direccion = (targetJugador.position - transform.position).normalized;
        direccion.y = 0;

        if (direccion != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    private void OnCollisionStay(Collision collision)
    {   //If the object the enemy collides with is the player, it kills him instantly
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerHealth componenteVidaJugador = collision.gameObject.GetComponent<PlayerHealth>();
        if (componenteVidaJugador != null)
        {
            componenteVidaJugador.Matar();
        }
    }
}