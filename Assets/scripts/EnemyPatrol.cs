using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Speed Between Waypoints")]
    [SerializeField] private float speed;

    //Here you set the Waypoints and Wait Time between Waypoints
    [Header("Waypoints/Rutines")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime;
    [SerializeField] private float rotationSpeed = 5.3f;
    private int currentWaypoint;
    private bool isWaiting;
    private bool isRotating;

    [Header("Configuración de Persecucin")]
    [SerializeField] private float velocidadPersecucion = 6f;
    private Transform targetJugador;
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
    public void IniciarPersecucion(Transform jugador)
    {
        //Detiene la rutina del patrullaje
        StopAllCoroutines();
        isWaiting = false;
        isRotating = false;

        //Activa el modo persecución
        targetJugador = jugador;
        estaPersiguiendo = true;

    }
    private void PerseguirAlJugador()
    {
        if (targetJugador == null) return;

        // Se mueve hacia la posición del jugador
        transform.position = Vector3.MoveTowards(transform.position, targetJugador.position, velocidadPersecucion * Time.deltaTime);

        // Rota suavemente para mirar al jugador mientras lo persigue
        Vector3 direction = (targetJugador.position - transform.position).normalized;
        direction.y = 0; //evita la inclinacion en rampas.. luego decidir si es mejor aclararlo en codigo o en el inspector.

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}