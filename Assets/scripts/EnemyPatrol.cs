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

    void Start()
    {
        Vector3 direction = (waypoints[0].position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
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
        int nextWaypoint;
        do nextWaypoint = Random.Range(0, waypoints.Length);
        while (nextWaypoint == currentWaypoint);
        currentWaypoint = nextWaypoint;
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
}