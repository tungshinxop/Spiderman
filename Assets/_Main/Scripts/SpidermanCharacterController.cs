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
    public TrailRenderer[] handTrails;
    
    [Header("Head")]
    [SerializeField] private Transform headCheckPos;
    [SerializeField] private CapsuleCollider capsuleCollider;
    
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
    public float coyoteTime = 0.25f;
    public float jumpBufferTime = 0.25f;
    [FormerlySerializedAs("jumpForce")] public float jumpHeight = 10f;
    public float fallMultiplier = 10f;
    
    
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
    private float _coyoteCounter;
    private float _jumpBufferCounter;
    private float _previousJumpHeight;

    [HideInInspector] public float JumpForce;    
    [HideInInspector] public bool IsGrounded;
    
    public bool PressedJump
    {
        get => _pressedJump;
        set => _pressedJump = value;
    }

    public Vector3 MoveInput => _moveInput;
    public float JumpBufferCounter
    {
        get => _jumpBufferCounter;
        set => _jumpBufferCounter = value;
    }

    void Start()
    {
        currentState = idleState;
        currentState.EnterState(this);
        _previousJumpHeight = jumpHeight;
        JumpForce = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight) + rb.drag;
    }

    void Update()
    {
        HandleInput();
        HandleRotation();
        HandleJumpInput();
        HandleJumpFeel();
        HandleGround();
        
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
        animator.SetFloat(AnimationHash.XAxis, _xBlend);
    }

    void HandleRotation()
    {
        if (_moveInput != Vector3.zero)
        {
            _rotation = Mathf.Atan2(_moveInput.x, _moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _rotation, ref _turnVelocity, turnTime);
        }
    }
    void HandleJumpInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Dont jump if there is a ceiling
            if (Physics.SphereCast(headCheckPos.position, capsuleCollider.radius, Vector3.up, out var hit, 1))
            {
                if (hit.collider != null) return;
            }

            if (Math.Abs(_previousJumpHeight - jumpHeight) > 0.1f)
            {
                _previousJumpHeight = jumpHeight;
                JumpForce = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight) + rb.drag;
            }
            
            _pressedJump = true;
            _jumpBufferCounter = jumpBufferTime;
        }
    }
    
    void HandleGround()
    {
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundRadius, groundLayer);
        rb.drag = IsGrounded ? groundDrag : airDrag;
        animator.SetBool(AnimationHash.Grounded, IsGrounded);
        if (!IsGrounded && rb.velocity.y <= 0)
        {
            rb.AddForce(new Vector3(0, -fallMultiplier, 0) * rb.mass, ForceMode.Acceleration);
        }
    }

    void HandleJumpFeel()
    {
        if (IsGrounded)
        {
            //reset coyote time on grounded
            _coyoteCounter = coyoteTime;
        }
        else
        {
            //start the timer for coyote effect
            _coyoteCounter -= Time.deltaTime;
            //clamping coyote timer
            _coyoteCounter = Mathf.Clamp(_coyoteCounter, 0, coyoteTime);
        }
        
        //start jump buffer timer
        _jumpBufferCounter -= Time.deltaTime;
        //clamping timer 
        _jumpBufferCounter = Mathf.Clamp(_jumpBufferCounter, 0, jumpBufferTime);
    }
    
    public bool IsOnSlope()
    {
        return false;
    }

    public bool IsValidJump()
    {
        return _jumpBufferCounter > 0 && _coyoteCounter > 0;
    }
    

    public void ResetVelocity()
    {
        var velocity = rb.velocity;
        velocity.y = 0f;
        rb.velocity = velocity;
    }

    public void Move()
    {
        var speed = IsGrounded ? groundSpeed * speedMultiplier : airSpeed * speedMultiplier;
        rb.AddForce(Quaternion.Euler(0.0f, _rotation, 0.0f) * Vector3.forward * speed, ForceMode.Acceleration);
    }

    public void SetToggleHandTrail(bool state)
    {
        foreach (var trail in handTrails)
        {
            trail.enabled = state;
        }
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
            DrawArrow.ForGizmo(groundCheckPos.position, cachedTransform.forward);
        }
    }
#endif
}
