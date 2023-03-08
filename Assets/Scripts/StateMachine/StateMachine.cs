using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected BaseState currentState;

    protected virtual void Awake()
    {
        Initialise();
    }

    public void Initialise()
    {
        if (currentState != null)
            currentState.Exit();
        currentState = GetInitialState();
        if (currentState != null)
            currentState.Enter();
    }

    public void Disable()
    {
        if (currentState != null)
            currentState.Exit();
        currentState = GetDisabledState();
        if (currentState != null)
            currentState.Enter();
    }

    protected virtual void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateLogic();
        }
    }

    public void ChangeState(BaseState newState)
    {
        currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }

    protected virtual BaseState GetDisabledState()
    {
        return null;
    }

    // kept for reference
    private void OnGUI()
    {
        if (Application.isEditor)
        {
            //string content = currentState != null ? currentState.name : "(no current state)";
            //GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
        }
    }
}
