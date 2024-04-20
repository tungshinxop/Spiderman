using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform cachedTransform;
    [SerializeField] float mouseSensitivity = 10;
    [SerializeField] Transform target;
    [SerializeField] float dstFromTarget = 2;
    [SerializeField] Vector2 pitchMinMax = new Vector2 (-40, 85);
    [SerializeField] float rotationSmoothTime = .12f;
    
    Vector3 _rotationSmoothVelocity;
    Vector3 _currentRotation;

    float _yaw;
    float _pitch;
    
    void LateUpdate () {
        _yaw += Input.GetAxis ("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis ("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp (_pitch, pitchMinMax.x, pitchMinMax.y);

        _currentRotation = Vector3.SmoothDamp (_currentRotation, new Vector3 (_pitch, _yaw), ref _rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = _currentRotation;

        cachedTransform.position = target.position - cachedTransform.forward * dstFromTarget;
    }
}
