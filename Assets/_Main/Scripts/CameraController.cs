using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    
    [SerializeField] private Transform cachedTransform;
    [SerializeField] float mouseSensitivity = 10;
    [SerializeField] Transform target;
    [SerializeField] float dstFromTarget = 2;
    [SerializeField] Vector2 pitchMinMax = new Vector2 (-40, 85);
    [SerializeField] float rotationSmoothTime = .12f;
    [SerializeField] private float zoomDuration = 0.75f;
    [SerializeField] private Camera cam;
    
    Vector3 _rotationSmoothVelocity;
    Vector3 _currentRotation;
    
    float _yaw;
    float _pitch;

    private Coroutine _camAction;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else{ Destroy(gameObject); }
        
        cachedTransform.position = target.position - cachedTransform.forward * dstFromTarget;
    }
    
    void LateUpdate () {
        _yaw += Input.GetAxis ("Mouse X") * mouseSensitivity;
        _pitch -= Input.GetAxis ("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch, pitchMinMax.x, pitchMinMax.y);

        _currentRotation = Vector3.SmoothDamp (_currentRotation, new Vector3 (_pitch, _yaw), ref _rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = _currentRotation;
    }

    public void Zoom(float dist, float fov)
    {
        if (_camAction != null)
        {
            StopCoroutine(_camAction);
        }
        _camAction = StartCoroutine(IECameraZoom(dist, fov));
    }
    
    public void ResetCamera()
    {
        Zoom(3.5f, 60f);
    }
    
    IEnumerator IECameraZoom(float dist, float fov)
    {
        var elapsedTime = 0f;
        var startFov = cam.fieldOfView;
        var startDist = dstFromTarget;
        while (elapsedTime < zoomDuration)
        {
            var t = elapsedTime / zoomDuration;
            cam.fieldOfView = Mathf.Lerp(startFov, fov, t);
            dstFromTarget = Mathf.Lerp(startDist, dist, t);
            cachedTransform.position = target.position - cachedTransform.forward * dstFromTarget;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cam.fieldOfView = fov;
        dstFromTarget = dist;
        cachedTransform.position = target.position - cachedTransform.forward * dstFromTarget;
        _camAction = null;
    }
}
