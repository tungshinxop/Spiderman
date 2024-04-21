using System;
using UnityEngine;
using Spiderman;

public class IdleState : BaseState
{
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.Grounded, SubStates.Idle);
        _manager.animator.SetBool(AnimationHash.Idle, true);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    protected override void ExitState()
    {
        _manager.animator.SetBool(AnimationHash.Idle, false);
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
        if (_manager.PressedJump && _manager.IsValidJump())
        {
            SwitchState(_manager.jumpState);
        }
        else
        {
            if (_manager.IsGrounded)
            {
                if (_manager.MoveInput != Vector3.zero)
                {
                    SwitchState(_manager.runState);
                }
            }
            else
            {
                SwitchState(_manager.fallState);
            }
        }
    }
}
