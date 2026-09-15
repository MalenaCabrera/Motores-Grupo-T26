using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNew : MonoBehaviour

{
    private Rigidbody rb;
    private PlayerInput playerInput;
    private Vector2 input;
    private float force = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

    }

    // Update is called once per frame
    void Update()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();
        //Debug.Log(input);
    }


    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(input.x, 0f, input.y) * force);
    }

}

//public class PlayerNew : MonoBehaviour
//{
//    private Rigidbody rb;
//    private PlayerInput playerInput;
//    private Vector2 input;
//    private float speed = 5f;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        playerInput = GetComponent<PlayerInput>();
//    }

//    void Update()
//    {
//        input = playerInput.actions["Move"].ReadValue<Vector2>();
//    }

//    private void FixedUpdate()
//    {
//        Vector3 velocity = new Vector3(input.x, 0f, input.y) * speed;
//        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
//    }
//}