using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

namespace ScapeRoom.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour, IPlayerSettings
    {
        [Header("References")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CapsuleCollider _col;
        [SerializeField] private Transform _playerCamera;
        [SerializeField] private Camera _cameraComponent;
        [SerializeField] private Transform _groundCheck;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _crouchSpeed = 2.5f;
        [SerializeField] private float _acceleration = 25f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _jumpForce = 1.5f;
        [SerializeField] private float _gravity = -15f; 
        [SerializeField] [Range(0f, 1f)] private float _airControl = 0.3f;
        
        [Header("Crouch")]
        [SerializeField] private float _standingHeight = 2f;
        [SerializeField] private float _crouchingHeight = 1f;

        [Header("Look")]
        [SerializeField] private float _mouseSensitivity = 1f;
        [SerializeField] private float _maxLookAngle = 85f;

        [Header("FOV Settings")]
        [SerializeField] private float _normalFOV = 60f;
        [SerializeField] private float _runFOV = 75f;
        [SerializeField] private float _fovLerpSpeed = 10f;

        [Header("Head Bob & Sway")]
        [SerializeField] private float _bobFrequency = 14f;
        [SerializeField] private float _bobAmplitude = 0.05f;
        [SerializeField] private float _swayAmount = 2f;
        [SerializeField] private float _tiltAmount = 2.5f;
        [SerializeField] private float _swaySmoothness = 10f;
        [SerializeField] private float _jumpBobAmount = 0.1f;
        [SerializeField] private float _landBobAmount = 0.2f;
        [SerializeField] private float _jumpBobSmoothness = 8f;

        [Header("Jump Assists")]
        [SerializeField] private float _coyoteTime = 0.15f;
        [SerializeField] private float _jumpBufferTime = 0.15f;

        [Header("Ground Check")]
        [SerializeField] private float _groundDistance = 0.4f;
        [SerializeField] private LayerMask _groundMask;

        public float WalkSpeed => _walkSpeed;
        public float RunSpeed => _runSpeed;
        public float CrouchSpeed => _crouchSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float AirControl => _airControl;
        public float JumpForce => _jumpForce;
        public float Gravity => _gravity;
        public float MouseSensitivity => _mouseSensitivity;
        public float MaxLookAngle => _maxLookAngle;
        public float StandingHeight => _standingHeight;
        public float CrouchingHeight => _crouchingHeight;
        public float BobFrequency => _bobFrequency;
        public float BobAmplitude => _bobAmplitude;
        public float SwayAmount => _swayAmount;
        public float TiltAmount => _tiltAmount;
        public float SwaySmoothness => _swaySmoothness;
        public float JumpBobAmount => _jumpBobAmount;
        public float LandBobAmount => _landBobAmount;
        public float JumpBobSmoothness => _jumpBobSmoothness;
        public float CoyoteTime => _coyoteTime;
        public float JumpBufferTime => _jumpBufferTime;

        private PlayerLocomotion _locomotion;
        private Vector2 _currentMoveInput;
        private Vector2 _currentLookInput;
        
        private float _coyoteTimer;
        private float _jumpBufferTimer;

        private bool _isCrouching;
        private bool _isSprinting;
        private bool _isGrounded;
        private float _baseCenterY;

        private bool _isGravityFlipping = false;

        private void Awake()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody>();
            if (_col == null) _col = GetComponent<CapsuleCollider>();
            if (_cameraComponent == null && _playerCamera != null) _cameraComponent = _playerCamera.GetComponent<Camera>();
            _locomotion = new PlayerLocomotion(this);
            _baseCenterY = _col.center.y;

            _rb.useGravity = false; 
            _rb.freezeRotation = true; 
            _rb.interpolation = RigidbodyInterpolation.None; 
            
            PhysicsMaterial pm = new PhysicsMaterial("PlayerZeroFriction");
            pm.dynamicFriction = 0f;
            pm.staticFriction = 0f;
            pm.frictionCombine = PhysicsMaterialCombine.Minimum;
            _col.material = pm;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (_playerCamera != null) _locomotion.InitializeCamera(_playerCamera);
        }

        private void OnMove(InputValue value) => _currentMoveInput = value.Get<Vector2>();
        private void OnLook(InputValue value) => _currentLookInput = value.Get<Vector2>();
        private void OnJump(InputValue value) { if (value.isPressed) _jumpBufferTimer = _jumpBufferTime; }
        private void OnCrouch(InputValue value) { _isCrouching = value.isPressed; }
        private void OnSprint(InputValue value)
        {
            _isSprinting = value.isPressed;
            if (_isSprinting && _isCrouching) _isCrouching = false;
        }

        private void Update()
        {
            if (_isGravityFlipping) return;

            HandleLook();
            HandleCrouch();
            HandleFOV();

            _locomotion.ApplyCameraBob(_isGrounded, _isCrouching, _playerCamera, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_isGravityFlipping) 
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            HandleMovement();
        }

        private void HandleLook()
        {
            if (_playerCamera == null) return;
            _locomotion.ApplyRotation(_currentLookInput, _currentMoveInput, transform, _playerCamera, Time.deltaTime);
        }

        private void HandleMovement()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            bool wasGrounded = _isGrounded;
            _isGrounded = _groundCheck != null && Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

            if (_coyoteTimer > 0) _coyoteTimer -= fixedDeltaTime;
            if (_jumpBufferTimer > 0) _jumpBufferTimer -= fixedDeltaTime;

            if (_isGrounded) _coyoteTimer = _coyoteTime;

            if (!wasGrounded && _isGrounded)
            {
                _locomotion.TriggerLandingBob();
            }

            bool actuallyJumping = false;
            if (_jumpBufferTimer > 0 && _coyoteTimer > 0 && !_isCrouching)
            {
                actuallyJumping = true;
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
                _locomotion.TriggerJumpBob();
            }

            Vector3 finalVelocity = _locomotion.CalculateVelocity(_currentMoveInput, transform, _isCrouching, _isSprinting, _isGrounded, actuallyJumping, fixedDeltaTime, _rb.linearVelocity);
            _rb.linearVelocity = finalVelocity;
        }

        private void HandleCrouch()
        {
            float targetHeight = _isCrouching ? _crouchingHeight : _standingHeight;
            _col.height = Mathf.Lerp(_col.height, targetHeight, Time.deltaTime * 10f);
            
            float targetCenterY = (targetHeight / 2f) + (_baseCenterY - (_standingHeight / 2f));
            Vector3 targetCenter = new Vector3(_col.center.x, targetCenterY, _col.center.z);
            _col.center = Vector3.Lerp(_col.center, targetCenter, Time.deltaTime * 10f);
        }

        private void HandleFOV()
        {
            if (_cameraComponent == null) return;
            bool isActuallyRunning = _isSprinting && !_isCrouching && _currentMoveInput.y > 0.1f;
            float targetFOV = isActuallyRunning ? _runFOV : _normalFOV;
            _cameraComponent.fieldOfView = Mathf.Lerp(_cameraComponent.fieldOfView, targetFOV, Time.deltaTime * _fovLerpSpeed);
        }

        public void FlipGravity()
        {
            if (_isGravityFlipping) return;
            StartCoroutine(FlipGravityRoutine());
        }

        private IEnumerator FlipGravityRoutine()
        {
            _isGravityFlipping = true;

            Physics.gravity = -Physics.gravity;

            Quaternion startRot = transform.rotation;
            Quaternion targetRot = startRot * Quaternion.Euler(0, 0, 180f);

            // Puedes ajustar este tiempo (1.0s) para que tarde más o menos en girar
            float duration = 0.5f; 
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Función Ease-In-Out para suavizar el inicio y el final de la rotación
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                
                transform.rotation = Quaternion.Slerp(startRot, targetRot, smoothT);
                yield return null;
            }
            
            transform.rotation = targetRot;
            _rb.linearVelocity = Vector3.zero;

            _isGravityFlipping = false;
        }
    }
}
