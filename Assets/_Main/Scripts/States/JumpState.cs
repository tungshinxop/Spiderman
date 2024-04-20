

using Spiderman;
using UnityEngine;

public class JumpState : BaseState
{
    private void FixedUpdate()
    {
        if(!gameObject.activeInHierarchy) return;
        if (_manager.MoveInput != Vector3.zero)
        {
            _manager.Move();
        }
    }
    
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.InAir, SubStates.Jump);
        _manager.animator.SetBool(AnimationHash.Jump, true);
        
        //reset velocity for a constant jump height
        _manager.PressedJump = false;
        _manager.ResetVelocity();
        _manager.JumpBufferCounter = 0;
        _manager.rb.AddForce(new Vector3(0, _manager.JumpForce , 0), ForceMode.Impulse);
    }

    protected override void ExitState()
    {
        _manager.animator.SetBool(AnimationHash.Jump, false);
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
        if (_manager.PointsToCheck.Count > 0)
        {
            SwitchState(_manager.swingState);
        }
        else
        {
            if (_manager.rb.velocity.y < 0) //velocity start decreasing => switch to fall state
            {
                SwitchState(_manager.fallState);
            }
        }
    }
}