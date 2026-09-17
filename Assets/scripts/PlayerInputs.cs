using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Inputs references")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;

    public bool crouch;
    public bool sprint;

    public float playerSensitivity = 1f;

    public void OnMove(InputAction.CallbackContext context)
    {
        Move(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookInput(context.ReadValue<Vector2>());       
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jump = true;
        else if (context.canceled)
            jump = false;
    }


    public void OnSprint(InputAction.CallbackContext context)
    {
        sprint = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        crouch = context.ReadValueAsButton();
    }


    public void Move(Vector2 newMove)
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

    public void Sprint(bool newSprint)
    {
        sprint = newSprint;
    }

    public void Crouch(bool newCrouch)
    {
        crouch = newCrouch;
    }
}
