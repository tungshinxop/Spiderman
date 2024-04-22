using System;
using Spiderman;
using UnityEngine;

public class WallRunState : BaseState
{
    private Vector3 _wallDirection;
    
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.InAir, SubStates.WallRun);
        _manager.animator.SetBool(AnimationHash.WallRun, true);
        _manager.rb.useGravity = false;
        _manager.ResetVelocity();
    }

    private void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_manager.MoveInput != Vector3.zero)
        {
            _manager.rb.AddForce(_wallDirection * (_manager.airSpeed * _manager.speedMultiplier), ForceMode.Acceleration);
        }
    }

    public override void UpdateState()
    {
        var lastNormal = _manager.LastWallNormal;
        var projectionRight = Vector3.zero;
        var projectionUp = Vector3.zero;
        // if (_manager.MoveInput.x > 0)
        // {
        //     projectionRight = Vector3.ProjectOnPlane(_manager.cachedTransform.right, lastNormal);
        // }
        // else if (_manager.MoveInput.x < 0)
        // {
        //     projectionRight = -Vector3.ProjectOnPlane(_manager.cachedTransform.right, lastNormal);
        // }

        if (_manager.MoveInput.x != 0)
        {
            projectionRight = Vector3.ProjectOnPlane(_manager.GetRotation() * Vector3.forward, lastNormal);
        }
        
        if (_manager.MoveInput.z > 0)
        {
            projectionUp = -Vector3.Cross(lastNormal,  Vector3.ProjectOnPlane(_manager.cachedTransform.right, lastNormal));
        }
        
        _wallDirection = (projectionRight + projectionUp).normalized; 
        _manager.cachedTransform.forward = -lastNormal;
        
        if (Application.isEditor)
        {
            DrawArrow.ForDebug(_manager.cachedTransform.position, _wallDirection, Color.magenta);
        }
        
        base.UpdateState();
    }

    protected override void ExitState()
    {
        _manager.animator.SetBool(AnimationHash.WallRun, false);
        _manager.rb.useGravity = true;
        _manager.ResetVelocity();
        if (_manager.PressedJump)
        {
            var dir = _manager.GetRotatedVector3(_manager.LastWallNormal, _manager.cachedTransform.right, 60f);
            _manager.rb.AddForce(dir.normalized * (_manager.JumpForce * 3f), ForceMode.Impulse);
        }
        else
        {
            _manager.rb.AddForce(_wallDirection * _manager.JumpForce, ForceMode.Impulse);
        }
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
        if (_manager.CanWallRun)
        {
            if (_manager.PressedJump)
            {
                SwitchState(_manager.jumpState);
            }
        }
        else
        {
            if (_manager.IsGrounded)
            {
                SwitchState(_manager.MoveInput == Vector3.zero ? _manager.idleState : _manager.runState);
            }
            else
            {
                SwitchState(_manager.fallState);
            }
        }
    }
}