﻿using System;
using UnityEngine;
using Spiderman;

public class IdleState : BaseState
{
    private int IdleHash = Animator.StringToHash("Idle");

    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.Grounded, SubStates.Idle);
        _manager.animator.SetTrigger(IdleHash);
    }

    public override void UpdateState()
    {
        CheckSwitchState();
    }

    protected override void ExitState()
    {
        _manager.animator.ResetTrigger(IdleHash);
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
        if (_manager.IsGrounded)
        {
            if (_manager.MoveDir != Vector3.zero)
            {
                SwitchState(_manager.runState);
            }
        }
        else
        {
            if (_manager.PressedJump)
            {
                SwitchState(_manager.jumpState);
            }
            else
            {
                SwitchState(_manager.fallState);
            }
        }
    }
}