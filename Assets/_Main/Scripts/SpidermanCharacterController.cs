using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum MainStates
{
    Grounded,
    InAir
}

public enum SubStates
{
    Run,
    Idle,
    Jump,
    Fall
}

public class SpidermanCharacterController : MonoBehaviour
{
    public Rigidbody rb;
    public Animator animator;
    public Transform cameraTransform;
    public Transform cachedTransform;
    
    [Header("Ground")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundRadius;
    [SerializeField] private LayerMask groundLayer;

    [Header("Data:")] 
    public float turnTime = 0.2f;
    public float speedMultiplier = 1f;
    public float groundSpeed;
    public float airSpeed;
    public float groundDrag = 4;
    public float airDrag = 2;
    public float xBlendRate = 10f;
    
    [Header("Air sub-states:")]
    public BaseState jumpState;
    public BaseState fallState;
    
    [Header("Ground sub-states:")]
    public BaseState runState;
    public BaseState idleState;

    [Header("Non-persistant data: ")]
    public BaseState currentState;
    
    private Vector3 _moveInput;
    private bool _pressedJump;
    private float _turnVelocity;
    private float _rotation;
    private float _xBlend;
    
    [HideInInspector] public bool IsGrounded;
    [HideInInspector] public int XAxisHash = Animator.StringToHash("XAxis");
    
    public bool PressedJump => _pressedJump;
    public Vector3 MoveInput => _moveInput;
    void Start()
    {
        currentState = idleState;
        currentState.EnterState(this);
    }

    void Update()
    {
        HandleInput();
        HandleRotation();
        CheckGround();
        
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    void HandleInput()
    {
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _moveInput = _moveInput.normalized;

        var targetValue = _moveInput.x == 0 ? 0 : _moveInput.x < 0 ? -1 : 1;
        _xBlend = Mathf.Lerp(_xBlend, targetValue, Time.deltaTime * xBlendRate);
        animator.SetFloat(XAxisHash, _xBlend);
    }

    private void CheckGround()
    {
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundRadius, groundLayer);
        rb.drag = IsGrounded ? groundDrag : airDrag;
    }
    
    void HandleJumpInput()
    {
        
    }
    
    void HandleRotation()
    {
        if (_moveInput != Vector3.zero)
        {
            _rotation = Mathf.Atan2(_moveInput.x, _moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _rotation, ref _turnVelocity, turnTime);
        }
    }
    
    public bool IsOnSlope()
    {
        return false;
    }

    public Vector3 MoveDir()
    {
        return Quaternion.Euler(0.0f, _rotation, 0.0f) * Vector3.forward;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundRadius);
            
            Gizmos.color = Color.red;
            if (_moveInput != Vector3.zero)
            {
                Gizmos.DrawRay(groundCheckPos.position, _moveInput.normalized * 5f);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(groundCheckPos.position, cachedTransform.forward);
        }
    }
#endif
}
