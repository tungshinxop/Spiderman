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
            _manager.Move();
        }
    }
    
    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.InAir, SubStates.Fall);
    }

    public override void UpdateState()
    {
        _timeFalling += Time.deltaTime;
        _manager.animator.SetFloat(AnimationHash.AirTime, _timeFalling);
        if (_timeFalling >= 0.35f && !_swappedToFreeFall)
        {
            _swappedToFreeFall = true;
            var rand = Random.Range(0, 3);
            _manager.animator.SetFloat(AnimationHash.RandomFallingAnim, rand);
            _manager.SetToggleHandTrail(true);
        }
        base.UpdateState();
    }

    protected override void ExitState()
    {
        _timeFalling = 0f;
        _manager.animator.SetFloat(AnimationHash.AirTime, _timeFalling);
        _manager.SetToggleHandTrail(false);
        _swappedToFreeFall = false;
        base.ExitState();
    }

    protected override void CheckSwitchState()
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