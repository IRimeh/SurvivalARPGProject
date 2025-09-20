using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private float _cameraSensitivity = .1f;
    [SerializeField] private float _cameraRotLerpSpeed = 10.0f;
    [SerializeField] private float _minCameraDistance;
    [SerializeField] private float _maxCameraDistance;
    [SerializeField] private float _cameraZoomSpeed;
    [SerializeField] private InputActionReference _controlActionReference;

    private float _currentCameraDistance;
    private Vector3 _defaultCameraRot;
    private Vector3 _lastMousePos;
    private float _cameraRot = 0;
    private Quaternion _targetLocalRot;
    
    private void Awake()
    {
        _defaultCameraRot = transform.localRotation.eulerAngles;
        _lastMousePos = Input.mousePosition;
        _currentCameraDistance = _maxCameraDistance;
        _targetLocalRot = Quaternion.Euler(_defaultCameraRot.x, _cameraRot, _defaultCameraRot.z);
    }

    private void Update()
    {
        if (_playerInventory.IsUIOpen)
        {
            SetCameraPosition();
        }
        else
        {
            RotateCamera();
            SetCameraPosition();
            CameraZoom();
        }
    }

    private void RotateCamera()
    {
        Vector2 mouseDelta = Input.mousePosition - _lastMousePos;
        if (Input.GetMouseButton(1))
        {
            _cameraRot += mouseDelta.x * _cameraSensitivity;
            _targetLocalRot = Quaternion.Euler(_defaultCameraRot.x, _cameraRot, _defaultCameraRot.z);
            _player.SetCameraRotation(_cameraRot);
        }
        _lastMousePos = Input.mousePosition;
        transform.localRotation = Quaternion.Slerp(transform.rotation, _targetLocalRot, Time.deltaTime * _cameraRotLerpSpeed);
    }

    private void SetCameraPosition()
    {
        transform.position = _player.transform.position + (-transform.forward * _currentCameraDistance);
    }

    private void CameraZoom()
    {
        if (!_controlActionReference.action.IsPressed())
            return;
        
        _currentCameraDistance = Mathf.Clamp(_currentCameraDistance - Input.mouseScrollDelta.y * _cameraZoomSpeed, _minCameraDistance, _maxCameraDistance);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        _currentCameraDistance = _maxCameraDistance;
        SetCameraPosition();
    }
}
