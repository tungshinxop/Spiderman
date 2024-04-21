using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    Fall,
    Swing
}

public class SpidermanCharacterController : MonoBehaviour
{
    //Poorly structured variables, to be updated (or not)
    public Rigidbody rb;
    public Animator animator;
    public Transform cameraTransform;
    public Transform cachedTransform;
    public TrailRenderer[] handTrails;
    public LineRenderer[] handWebs;
    public Transform model;

    [Header("Web detector")] 
    [SerializeField] private Transform webDetectorPos;
    [SerializeField] private int numberOfRays = 10;
    [SerializeField] private Vector2 angleRange = new Vector2(90, 30);
    [SerializeField] private float durationCheck = 0.75f;
    [SerializeField] private float detectorRange = 30f;
    
    [Header("Head")]
    [SerializeField] private Transform headCheckPos;
    [SerializeField] private CapsuleCollider capsuleCollider;
    
    [Header("Ground")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private Transform slopeCheckPos;
    [SerializeField] private float groundRadius;
    [SerializeField] private float slopeCheckDist;
    [SerializeField] private LayerMask groundLayer;

    [Header("Data:")] 
    public float turnTime = 0.2f;
    public float speedMultiplier = 1f;
    public float groundSpeed;
    public float airSpeed;
    public float swingSpeed = 7f;
    public float groundDrag = 4;
    public float airDrag = 2;
    public float xBlendRate = 10f;
    public float coyoteTime = 0.25f;
    public float jumpBufferTime = 0.25f;
    public float jumpHeight = 10f;
    public float fallMultiplier = 10f;
    public float walkableSlopeAngle = 45f;
    public float exitSwingForce = 30f;
    
    
    [Header("Air sub-states:")]
    public BaseState jumpState;
    public BaseState fallState;
    public BaseState swingState;
    
    [Header("Ground sub-states:")]
    public BaseState runState;
    public BaseState idleState;

    [Header("Non-persistant data: ")]
    public BaseState currentState;

    private RaycastHit _slopeHit;
    private Vector3 _moveInput;
    private bool _pressedJump;
    private float _turnVelocity;
    private float _rotation;
    private float _xBlend;
    private float _coyoteCounter;
    private float _jumpBufferCounter;
    private float _previousJumpHeight;
    private List<Vector3> _pointsToCheck = new List<Vector3>();

    [HideInInspector] public float JumpForce;    
    [HideInInspector] public bool IsGrounded;
    [HideInInspector] public bool IsOnSlope;
    [HideInInspector] public bool HoldingMouse;
    [HideInInspector] public bool PreSwingState;
    [HideInInspector] public float SwingCooldown;
    
    public List<Vector3> PointsToCheck => _pointsToCheck;

    public bool PressedJump
    {
        get => _pressedJump;
        set => _pressedJump = value;
    }

    public Vector3 MoveInput
    {
        get => _moveInput;
        set => _moveInput = value;
    }

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
        HandleSlope();
        HandleRotation();
        HandleJumpInput();
        HandleJumpFeel();
        HandleGround();
        HandleWebDetectionInput();
        
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
            RaycastHit hit;
            if (Physics.Raycast(slopeCheckPos.position, Vector3.down, out hit,Mathf.Infinity, groundLayer))
            {
                animator.SetFloat(AnimationHash.DistFromGround, hit.distance);
            }
            else
            {
                animator.SetFloat(AnimationHash.DistFromGround, -1);
            }
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
    
    private void HandleSlope()
    {
        //cast a ray down to find the normal of the ground to determine if it is a slope
        if (IsGrounded && Physics.Raycast(slopeCheckPos.position, Vector3.down, out _slopeHit, capsuleCollider.height + .2f, groundLayer))
        {
            if (_slopeHit.normal != Vector3.up)
            {
                var angle = Vector3.Angle(_slopeHit.normal, Vector3.up);
                if (angle <= walkableSlopeAngle && angle >= 5f)
                {
                    rb.useGravity = false;
                    IsOnSlope = true;
                    return;
                }
            }
        }
        
        rb.useGravity = true;
        IsOnSlope = false;
    }

    private void HandleWebDetectionInput()
    {
        if(SwingCooldown >= 0)
        {
            SwingCooldown -= Time.deltaTime;
        }
        
        HoldingMouse = Input.GetMouseButton(0);
        var position = cachedTransform.position;
        webDetectorPos.position = new Vector3(position.x, position.y + 10f, position.z);
        
        //Input detection
        if (HoldingMouse && !PreSwingState && currentState != swingState && !IsGrounded && SwingCooldown <= 0)
        {
            StartCoroutine(IEDetectWeb());
        }
    }

    private IEnumerator IEDetectWeb()
    {
        PreSwingState = true;
        var cachedWebDetector = webDetectorPos.position;
        var cachedWebDetectorForward = webDetectorPos.forward;
        _pointsToCheck.Clear();
        var temp = new List<bool>();
        var elapsedTime = 0f;
        var angle = angleRange.x;
        var angleBtwRays = 360f / numberOfRays;
        while (elapsedTime < durationCheck)
        {
            angle = Mathf.Lerp( angleRange.x,  angleRange.y, elapsedTime / durationCheck);
            for (int i = 0; i < numberOfRays; i++)
            {
                if (i + 1 > temp.Count)
                {
                    temp.Add(new bool());
                    temp[^1] = false;
                }

                if (temp[i])
                {
                    continue;
                }
                
                var upward = GetRotatedVector3(cachedWebDetectorForward, Vector3.up, angleBtwRays * i);
                var crossProduct = Vector3.Cross(upward, Vector3.up);
                var dir = GetRotatedVector3(upward, crossProduct, angle);
                RaycastHit hit;
                if (Physics.Raycast(cachedWebDetector, dir.normalized,  out hit, detectorRange,groundLayer))
                {
                    //Store available points
                    temp[i] = true;
                    _pointsToCheck.Add(hit.point);
                }
                
                if (Application.isEditor)
                {
                    Debug.DrawRay(cachedWebDetector, dir.normalized * detectorRange, hit.collider == null ? Color.red: Color.yellow);
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (_pointsToCheck.Count <= 0)
        {
            Vector3 fakePoint = cachedTransform.position + (cachedTransform.position.y > 100f ? Vector3.down * 20f  : Vector3.up * 20f) + 
                                GetRotatedVector3(cachedTransform.forward, Vector3.up, Random.Range(-15f,15f)) * 14f;
            _pointsToCheck.Add(fakePoint);
        }
    }
    
    public bool IsValidJump()
    {
        return _jumpBufferCounter > 0 && _coyoteCounter > 0;
    }

    public bool IsFiredWeb()
    {
        return _pointsToCheck.Count > 0 && PreSwingState;
    }
    
    public void ResetVelocity()
    {
        var velocity = rb.velocity;
        velocity.y = 0f;
        rb.velocity = velocity;
    }

    public void Move(float speed)
    {
        speed *= speedMultiplier;
        var dir = GetRotation() * Vector3.forward;
        if (IsOnSlope)
        {
            dir = Vector3.ProjectOnPlane(GetRotation() * Vector3.forward, _slopeHit.normal).normalized;
        }
        rb.AddForce(dir * speed, ForceMode.Acceleration);
    }

    public Quaternion GetRotation()
    {
        return Quaternion.Euler(0.0f, _rotation, 0.0f);
    }

    public void SetToggleHandTrail(bool state)
    {
        foreach (var trail in handTrails)
        {
            trail.enabled = state;
        }
    }

    private Coroutine _resetModelRoutine;
    
    public void ResetModelRotation()
    {
        if (_resetModelRoutine != null)
        {
            StopCoroutine(_resetModelRoutine);
        }

        _resetModelRoutine = StartCoroutine(IEResetModel());
    }
    
    IEnumerator IEResetModel()
    {
        var elapsedTime = 0f;
        var duration = 0.25f;
        while (elapsedTime < duration)
        {
            model.rotation = Quaternion.Slerp(model.rotation, model.parent.rotation, elapsedTime/ duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        model.rotation = model.parent.rotation;
        _resetModelRoutine = null;
    }
    
    public Vector3 GetRotatedVector3(Vector3 originalVector, Vector3 axisToRotateAround, float rotateAngle)
    {
        return Quaternion.AngleAxis(rotateAngle, axisToRotateAround) * originalVector;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (groundCheckPos != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPos.position, groundRadius);

            Gizmos.color = Color.blue;
            DrawArrow.ForGizmo(groundCheckPos.position, cachedTransform.forward);
            
            Gizmos.color = IsOnSlope ? Color.green : Color.red;
            DrawArrow.ForGizmo(slopeCheckPos.position, Vector3.down * slopeCheckDist);
            
            if (IsOnSlope)
            {
                Gizmos.color = new Color(0f, 0.98f, 1f);
                DrawArrow.ForGizmo(groundCheckPos.position, Vector3.ProjectOnPlane(Quaternion.Euler(0.0f, _rotation, 0.0f) * Vector3.forward, _slopeHit.normal).normalized);
                DrawArrow.ForGizmo(_slopeHit.point, _slopeHit.normal);
            }

            if (_pointsToCheck != null && _pointsToCheck.Count > 0)
            {
                foreach (var point in _pointsToCheck)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(point, 0.35f);
                }
            }
            
            Gizmos.color = Color.blue;
            DrawArrow.ForGizmo(headCheckPos.position, model.forward);
            Gizmos.color = Color.green;
            DrawArrow.ForGizmo(headCheckPos.position, model.up);
        }
    }
#endif
}
