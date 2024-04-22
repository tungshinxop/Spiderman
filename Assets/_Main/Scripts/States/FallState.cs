using Spiderman;
using UnityEngine;

public class FallState : BaseState
{
    private float _timeFalling;
    private bool _swappedToFreeFall;
    private void FixedUpdate()
    {
        if(!gameObject.activeInHierarchy) return;
        if (_manager.MoveInput != Vector3.zero)
        {
            _manager.Move(_manager.airSpeed);
        }
    }
    
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.InAir, SubStates.Fall);
        var rand = Random.Range(0, 3);
        _manager.animator.SetFloat(AnimationHash.RandomFallingAnim, rand);
        _manager.ResetVelocity();
    }

    public override void UpdateState()
    {
        _timeFalling += Time.deltaTime;
        _manager.animator.SetFloat(AnimationHash.AirTime, _timeFalling);
        if (_timeFalling >= 0.35f && !_swappedToFreeFall)
        {
            _swappedToFreeFall = true;
            _manager.SetToggleHandTrail(true);
            CameraController.Instance.Zoom(3.5f, 50f);
        }
        _manager.UpdateAirLogic();
        base.UpdateState();
    }

    protected override void ExitState()
    {
        _timeFalling = 0f;
        _manager.animator.SetFloat(AnimationHash.AirTime, _timeFalling);
        _manager.SetToggleHandTrail(false);
        _swappedToFreeFall = false;
        CameraController.Instance.ResetCamera();
        base.ExitState();
    }

    protected override void CheckSwitchState()
    {
        if (_manager.CanWallRun)
        {
            SwitchState(_manager.wallRun);
            return;
        }
        
        if (_manager.IsFiredWeb())
        {
            SwitchState(_manager.swingState);
        }
        else
        {
            if (_manager.IsGrounded)
            {
                if (_manager.MoveInput == Vector3.zero)
                {
                    SwitchState(_manager.idleState);
                }
                else
                {
                    SwitchState(_manager.runState);
                }
            }
        }

    }
}