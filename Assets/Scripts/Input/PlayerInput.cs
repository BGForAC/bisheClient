using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, InputActions.IGamePlayActions
{
    public static event UnityAction onMoveLeft = delegate { };
    public static event UnityAction onMoveRight = delegate { };
    public static event UnityAction onAcc = delegate { };
    public static event UnityAction onRotate = delegate { };
    public static event UnityAction onCancelAcc = delegate { };

    static InputActions inputActions;

    void Awake()
    {
        inputActions = new InputActions();
        inputActions.GamePlay.SetCallbacks(this);
    }

    void OnEnable()
    {
        EnableGamePlayInputs(); 
    }

    void OnDisable()
    {
        DisableAllInputs();
    }

    public static void EnableGamePlayInputs()
    {
        inputActions.GamePlay.Enable();
    }

    public static void DisableAllInputs()
    {
        inputActions.GamePlay.Disable();
    }

    public void OnAcc(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onAcc?.Invoke();
        }
        if (context.canceled)
        {
            onCancelAcc?.Invoke();
        }
    }

    public void OnMoveLeft(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onMoveLeft?.Invoke();
        }
    }

    public void OnMoveRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onMoveRight?.Invoke();
        }
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onRotate?.Invoke();
        }
    }
}