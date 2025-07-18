using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ItemData _rockData;
    public ItemManager _itemManager;
    
    [Header("References")]
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private Rigidbody _rigidBody;
    [SerializeField]
    private Transform _modelParent;
    [SerializeField] 
    private ParticleSystem _runParticles;
    [SerializeField] 
    private ParticleSystem _jumpParticles;
    [SerializeField]
    private LayerMask _climbMask;

    [Header("Movement properties")]
    [SerializeField]
    private float _maxSpeed = 5.0f;
    [SerializeField] 
    private float _sprintMaxSpeed = 22.5f;
    [SerializeField]
    private float _acceleration = 10.0f;
    [SerializeField]
    private float _rotateSpeed = 10.0f;
    [SerializeField] 
    private float _jumpForce = 50.0f;
    [SerializeField] 
    private float _climbingSpeed = 10.0f;

    [Header("Internal vars")] 
    [SerializeField] 
    private float _groundCheckDist = 1.0f;
    [SerializeField] 
    private float _jumpCooldown = 0.1f;

    public float MaxSpeed
    {
        get { return _isSprinting ? _sprintMaxSpeed : _maxSpeed; }
    }

    private bool _lastFrameSprinting = false;
    private Vector3 _targetVelocity;
    private Transform _rotationLookAtTransform;
    private float _timeGrounded = 0.0f;
    
    private bool _isSprinting = false;
    private bool _isGrounded = false;
    private float _cameraRotation = 0.0f;

    private Transform _climbingTransform = null;
    private bool _isTryingToClimb = false;
    private bool _isClimbing = false;

    private List<Collider> _triggerColliders = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        _rotationLookAtTransform = new GameObject("RotationLookAtTransform").transform;
        _rotationLookAtTransform.SetParent(transform);
        _rotationLookAtTransform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();
        UpdateAnimations();
        CheckGrounded();
        Climbing();
        UpdateVelocity();
        UpdateVFX();
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            _itemManager.SpawnItem(_rockData, 1, transform.position + Vector3.up, 1);
        }
        if (Input.GetKey(KeyCode.P))
        {
            _itemManager.SpawnItem(_rockData, 1, transform.position + Vector3.up, 1);
        }
    }

    private void Climbing()
    {
        if (!_isTryingToClimb)
        {
            if(_isClimbing)
                StopClimbing();
            return;
        }
        
        if (!_isClimbing)
            StartClimbing();

        if (!_isClimbing)
            return;

        _targetVelocity.y = _isTryingToClimb ? _climbingSpeed : 0;
    }

    private void StartClimbing()
    {
        if (_triggerColliders.Count <= 0)
            return;

        _climbingTransform = _triggerColliders[0].transform;
        _isClimbing = true;
        _rigidBody.useGravity = false;
    }

    private void StopClimbing()
    {
        _climbingTransform = null;
        _isClimbing = false;
        _rigidBody.useGravity = true;
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics.Raycast(transform.position + (Vector3.up * (_groundCheckDist * .5f)), Vector3.down, _groundCheckDist);
        _animator.SetBool("IsJumping", !_isGrounded);
        if (!_isGrounded)
        {
            _runParticles.Stop();
            _timeGrounded = 0.0f;
        }

        if (_isGrounded)
        {
            _timeGrounded += Time.deltaTime;
            StopClimbing();
        }
    }

    private void Inputs()
    {
        Vector3 targetDir = Vector3.zero;
        if(Input.GetKey(KeyCode.W))
            targetDir.z += 1;
        if (Input.GetKey(KeyCode.S))
            targetDir.z -= 1;
        if (Input.GetKey(KeyCode.D))
            targetDir.x += 1;
        if (Input.GetKey(KeyCode.A))
            targetDir.x -= 1;
        
        _isSprinting = Input.GetKey(KeyCode.LeftShift);
        if (_isSprinting != _lastFrameSprinting)
        {
            if (_isSprinting)
                StartSprinting();
            else
                StopSprinting();
            _lastFrameSprinting = _isSprinting;
        }

        bool holdingSpace = Input.GetKey(KeyCode.Space);
        _isTryingToClimb = holdingSpace && !_isGrounded;

        if (holdingSpace && _isGrounded)
            Jump(1);
        
        if (Input.GetKey(KeyCode.C) && _isGrounded)
            Jump(3);

        targetDir = Quaternion.Euler(0, _cameraRotation, 0) * targetDir;
        targetDir.Normalize();
        _targetVelocity = new Vector3(MaxSpeed * targetDir.x, _rigidBody.velocity.y, MaxSpeed * targetDir.z);
    }

    private void UpdateVelocity()
    {
        _rigidBody.velocity = Vector3.Lerp(_rigidBody.velocity, _targetVelocity, _acceleration * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        // Rotation
        _rotationLookAtTransform.transform.position = transform.position + new Vector3(_targetVelocity.x, 0, _targetVelocity.z);
        Quaternion rot = _modelParent.transform.rotation;
        _modelParent.LookAt(_rotationLookAtTransform, Vector3.up);
        _modelParent.transform.rotation = Quaternion.Slerp(rot, _modelParent.transform.rotation, Time.deltaTime * _rotateSpeed);
        
        // Set animator parameters
        _animator.SetFloat("Speed", _targetVelocity.magnitude);
    }

    private void Jump(float multiplier = 1)
    {
        if (_isGrounded && _timeGrounded >= _jumpCooldown)
        {
            Vector3 force = Vector3.up * _jumpForce;
            force *= multiplier;
            _rigidBody.AddForce(force);
            _isGrounded = false;
            _runParticles.Stop();
            _timeGrounded = 0.0f;
            _jumpParticles.Play();
        }
    }

    private void UpdateVFX()
    {
        if (_isGrounded && _isSprinting && !_runParticles.isPlaying)
            _runParticles.Play();
    }

    private void StartSprinting()
    {
        if(_isGrounded)
            _runParticles.Play();
    }

    private void StopSprinting()
    {
        _runParticles.Stop();
    }

    public void SetCameraRotation(float cameraRotation)
    {
        _cameraRotation = cameraRotation;
    }


    private void OnTriggerEnter(Collider other)
    {
        if((_climbMask & (1 << other.gameObject.layer)) != 0)
            _triggerColliders.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        _triggerColliders.Remove(other);
        
        if(other.transform == _climbingTransform)
            StopClimbing();
    }
}
