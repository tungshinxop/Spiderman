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

    public int XAxisHash = Animator.StringToHash("XAxis");
    
    [Header("Ground")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundRadius;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Data:")] 
    public float speedMultiplier = 1f;
    public float groundSpeed;
    public float airSpeed;
    public float groundDrag;
    public float airDrag;
    
    [Header("Air sub-states:")]
    public BaseState jumpState;
    public BaseState fallState;
    
    [Header("Ground sub-states:")]
    public BaseState runState;
    public BaseState idleState;

    public BaseState currentState;
    
    private Vector3 _moveInput;
    private bool _pressedJump;

    [HideInInspector] public bool IsGrounded;
    
    public Vector3 MoveInput => _moveInput;
    public bool PressedJump => _pressedJump;
    void Start()
    {
        currentState = idleState;
        currentState.EnterState(this);
    }

    void Update()
    {
        HandleInput();
        CheckGround();
        
        if (currentState != null)
        {
            currentState.UpdateState();
        }
    }

    void HandleInput()
    {
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if (_moveInput.x < 0)
        {
            animator.SetFloat(XAxisHash, -1);
        }
        else if(_moveInput.x > 0)
        {
            animator.SetFloat(XAxisHash, 1);
        }
        else
        {
            animator.SetFloat(XAxisHash, 0);
        }
    }

    private void CheckGround()
    {
        //cast a sphere to check grounded
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundRadius, groundLayer);
    }
    
    void HandleJumpInput()
    {
        
    }
    
    void HandleRotation()
    {
        
    }
    
    public bool IsOnSlope()
    {
        return false;
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
        }
    }
#endif
}
