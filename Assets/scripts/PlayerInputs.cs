using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Inputs references")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;

    public float playerSensitivity = 1f;

    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value)
    {
        LookInput(value.Get<Vector2>());       
    }

    public void OnJump(InputValue value)
    {
        JumpInput(value.isPressed);
    }

    public void MoveInput(Vector2 newMove)
    { 
       move = newMove;
    }
    public void LookInput(Vector2 newLook)
    {
        look = newLook;
    }

    public void JumpInput(bool stateJump)
    {
        jump = stateJump;
    }


}
