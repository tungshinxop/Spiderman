using Spiderman;
using UnityEngine;

public class RunState : BaseState
{
    private void FixedUpdate()
    {
        if(!gameObject.activeInHierarchy) return;
        _manager.Move();
    }
    
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.Grounded, SubStates.Run);
        _manager.animator.SetBool(AnimationHash.Run, true);
    }

    protected override void ExitState()
    {
        _manager.animator.SetBool(AnimationHash.Run, false);
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
            if (_manager.PressedJump && _manager.IsValidJump())
            {
                SwitchState(_manager.jumpState);
            }
            else
            {
                if (_manager.IsGrounded)
                {
                    if (_manager.PressedJump && _manager.IsValidJump())
                    {
                        SwitchState(_manager.jumpState);
                    }
                
                    if (_manager.MoveInput == Vector3.zero)
                    {
                        SwitchState(_manager.idleState);
                    }
                }
                else
                {
                    SwitchState(_manager.fallState);
                }
            }
        }
        
    }
}
