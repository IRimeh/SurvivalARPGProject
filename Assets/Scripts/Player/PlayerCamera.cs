using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private float _cameraDistance;
    [SerializeField] private float _cameraSensitivity = .1f;
    [SerializeField] private float _cameraRotLerpSpeed = 10.0f;

    private Vector3 _defaultCameraRot;
    private Vector3 _lastMousePos;
    private float _cameraRot = 0;
    private Quaternion _targetLocalRot;
    
    private void Awake()
    {
        _defaultCameraRot = transform.localRotation.eulerAngles;
        _lastMousePos = Input.mousePosition;
    }

    private void Update()
    {
        RotateCamera();
        SetCameraPosition();
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
        transform.position = _player.transform.position + (-transform.forward * _cameraDistance);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;
        
        SetCameraPosition();
    }
}
