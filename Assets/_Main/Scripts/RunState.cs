using Spiderman;
using UnityEngine;

public class RunState : BaseState
{
    private int RunHash = Animator.StringToHash("Run");
    
    private void FixedUpdate()
    {
        if(!gameObject.activeInHierarchy) return;
        
        var speed = _manager.groundSpeed * _manager.speedMultiplier;
        _manager.rb.AddForce(_manager.MoveDir.normalized * speed , ForceMode.Acceleration);
        
        // if (_manager.IsOnSlope())
        // else
        //     _manager.rb.AddForce(_slopeDirection.normalized * speed, ForceMode.Acceleration);
    }
    
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.Grounded, SubStates.Run);
        _manager.animator.SetTrigger(RunHash);
    }

    protected override void ExitState()
    {
        _manager.animator.ResetTrigger(RunHash);
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
        if (_manager.IsGrounded)
        {
            if (_manager.MoveDir == Vector3.zero)
            {
                SwitchState(_manager.idleState);
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
