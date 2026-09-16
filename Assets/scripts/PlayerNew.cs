using UnityEditor.Build;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNew : MonoBehaviour
{

    private float force = 10f;
    private Rigidbody rb;
    private PlayerInput playerInput;
    public CharacterController controller;
    private Vector2 input;
    public float speed = 10f;



    void Start()
    {
       rb = GetComponent<Rigidbody>();
       playerInput = GetComponent<PlayerInput>();
    }

    
    void Update()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();
    }
    private void FixedUpdate()
    {
        rb.AddForce(new Vector3(input.x, 0f, input.y) * force);
    }

    //public void Move(InputAction.CallbackContext callbackContext)
    //{
    //    if (callbackContext.performed)
    //    {
    //        Vector2 input = callbackContext.ReadValue<Vector2>();
    //        rb.AddForce(new Vector3(input.x, 0f, input.y) * force);
    //    }

    //}



}