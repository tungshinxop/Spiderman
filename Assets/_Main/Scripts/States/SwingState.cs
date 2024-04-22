using System;
using System.Collections;
using Spiderman;
using Unity.VisualScripting;
using UnityEngine;

public class SwingState : BaseState
{
    private Vector3 _optimalPoint;
    private bool _initializedOptimalPoint;
    private float _currentSwingTime;
    private int _handIndex = -1;
    private float _distance;
    private Vector3 _swingDirection;
    private Vector3 _currentForward;

    private void FixedUpdate()
    {
        if (!gameObject.activeInHierarchy || !_initializedOptimalPoint || _handIndex < 0) return;
        _manager.rb.AddForce(_swingDirection * (_manager.swingSpeed * _manager.speedMultiplier), ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        if (!gameObject.activeInHierarchy || !_initializedOptimalPoint || _handIndex < 0) return;
        DrawWeb();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        
        if (!gameObject.activeInHierarchy || !_initializedOptimalPoint || _handIndex < 0) return;
        
        var dirToPoint = (_optimalPoint - _manager.cachedTransform.position).normalized;
        _currentForward = Vector3.Cross(_manager.cachedTransform.right , dirToPoint).normalized;
        _swingDirection = (dirToPoint + _currentForward).normalized;
        
        _manager.model.forward = Vector3.Slerp(_manager.model.forward, _currentForward, 3f * Time.deltaTime);
        if (Application.isEditor)
        {
            DrawArrow.ForDebug(_manager.cachedTransform.position, _currentForward, Color.red);
            DrawArrow.ForDebug(_manager.cachedTransform.position, _swingDirection, Color.yellow);
        }
        
        _currentSwingTime += Time.deltaTime;
    }

    private void DrawWeb()
    {
        var lineRenderer = _manager.handWebs[_handIndex];
        if (lineRenderer.positionCount <= 0)
        {
            lineRenderer.positionCount = 2;
        }

        lineRenderer.SetPosition(0, _optimalPoint);
        lineRenderer.SetPosition(1, lineRenderer.transform.position);
    }

    public override void EnterState(SpidermanCharacterController manager)
    {
        base.EnterState(manager);
        Debugger.Instance.UpdateCurrentStateDebugger(MainStates.InAir, SubStates.Swing);
        _manager.animator.SetBool(AnimationHash.Swing, true);
        _currentSwingTime = 0;
        _manager.ResetVelocity();
        _manager.PlayWebAudio();
        CameraController.Instance.Zoom(5f, 75f);
        FindOptimalPoint();
        HandleWebShootAnim();
    }
    
    protected override void ExitState()
    {
        _manager.animator.SetBool(AnimationHash.Swing, false);
        _initializedOptimalPoint = false;
        if (!_manager.IsGrounded && _currentSwingTime > 0.1f)
        {
            _manager.ResetVelocity();
            _manager.rb.AddForce(_currentForward * _manager.exitSwingForce, ForceMode.Impulse);
        }
        _manager.handWebs[_handIndex].positionCount = 0;
        _handIndex = -1;
        _currentSwingTime = 0;
        _distance = -1;
        _manager.PreSwingState = false;
        _manager.SwingCooldown = 0.65f;
        _manager.ResetModelRotation();
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
        else
        {
            if (_manager.HoldingMouse)
            {
                var dot = Vector3.Dot(_manager.cachedTransform.forward, _swingDirection);
                if (_currentSwingTime > 5f || (dot < -0.5f && _currentSwingTime > 0.1f))
                {
                    SwitchState(_manager.fallState);
                }
            }
            else
            {
                if (_currentSwingTime > 0.2f)
                {
                    SwitchState(_manager.fallState);
                }
            }       
        }
    }

    private void FindOptimalPoint()
    {
        _optimalPoint = _manager.PointsToCheck[0];
        _distance = Vector3.Distance(_manager.cachedTransform.position, _manager.PointsToCheck[0]);
        for (int i = 1; i < _manager.PointsToCheck.Count; i++)
        {
            var curDist = Vector3.Distance(_manager.cachedTransform.position, _manager.PointsToCheck[i]);
            if (curDist < _distance)
            {
                _distance = curDist;
                _optimalPoint = _manager.PointsToCheck[i];
            }
        }
        _manager.PointsToCheck.Clear();
        _initializedOptimalPoint = true;
    }

    private void HandleWebShootAnim()
    {
        _handIndex = IsLeft(_optimalPoint, _manager.cachedTransform.forward) ? 0 : 1;
        _manager.animator.SetFloat(AnimationHash.CurrentHand, _handIndex);
        DrawWeb();
    }
    
    private bool IsLeft(Vector3 targetPoint, Vector3 objectForward)
    {
        Vector3 targetVector = targetPoint - transform.position;
        Vector3 perpVector = Vector3.Cross(objectForward, Vector3.up);
        float dotProduct = Vector3.Dot(targetVector, perpVector);
        return (objectForward.x > 0f) ? dotProduct > 0f : dotProduct < 0f;
    }
}
