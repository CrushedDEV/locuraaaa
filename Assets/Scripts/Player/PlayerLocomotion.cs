using UnityEngine;

namespace ScapeRoom.Player
{
    public class PlayerLocomotion
    {
        private readonly IPlayerSettings _settings;
        private float _xRotation;
        private float _zRotation;
        private Vector3 _currentVelocity;
        private float _bobTimer;
        private float _defaultCameraY;
        private float _jumpLandBobOffset;

        public PlayerLocomotion(IPlayerSettings settings)
        {
            _settings = settings;
        }

        public void InitializeCamera(Transform cameraTransform)
        {
            if (cameraTransform == null) return;
            float currentX = cameraTransform.localEulerAngles.x;
            if (currentX > 180f) currentX -= 360f; 
            _xRotation = currentX;
            _defaultCameraY = cameraTransform.localPosition.y;
        }

        public Vector3 CalculateVelocity(Vector2 moveInput, Transform playerTransform, bool isCrouching, bool isSprinting, bool isGrounded, bool actuallyJumping, float deltaTime, Vector3 currentRbVelocity)
        {
            float targetSpeed = _settings.WalkSpeed;
            if (isCrouching) targetSpeed = _settings.CrouchSpeed;
            else if (isSprinting && moveInput.y > 0.1f) targetSpeed = _settings.RunSpeed;

            Vector3 moveDirection = (playerTransform.right * moveInput.x + playerTransform.forward * moveInput.y).normalized;
            Vector3 targetVelocity = moveDirection * targetSpeed;

            float baseAccel = moveInput.sqrMagnitude > 0 ? _settings.Acceleration : _settings.Deceleration;
            float finalAccel = isGrounded ? baseAccel : baseAccel * _settings.AirControl;

            Vector3 localCurrentVel = playerTransform.InverseTransformDirection(currentRbVelocity);
            Vector3 localTargetVel = playerTransform.InverseTransformDirection(targetVelocity);
            
            Vector3 planarCurrentVel = new Vector3(localCurrentVel.x, 0, localCurrentVel.z);
            Vector3 planarTargetVel = new Vector3(localTargetVel.x, 0, localTargetVel.z);

            Vector3 newPlanarVel = Vector3.MoveTowards(planarCurrentVel, planarTargetVel, finalAccel * deltaTime);
            _currentVelocity = playerTransform.TransformDirection(newPlanarVel);

            // Apply existing gravity correctly matching its current global force multiplier
            Vector3 gravityForce = Physics.gravity * deltaTime;
            Vector3 finalWorldVelocity = _currentVelocity + playerTransform.up * localCurrentVel.y + gravityForce;

            if (actuallyJumping)
            {
                float jumpSpeed = Mathf.Sqrt(_settings.JumpForce * 2f * Mathf.Abs(_settings.Gravity));
                finalWorldVelocity = _currentVelocity + playerTransform.up * jumpSpeed;
            }

            return finalWorldVelocity;
        }

        public void ApplyRotation(Vector2 lookInput, Vector2 moveInput, Transform playerTransform, Transform cameraTransform, float deltaTime)
        {
            float mouseX = lookInput.x * _settings.MouseSensitivity * 0.1f;
            float mouseY = lookInput.y * _settings.MouseSensitivity * 0.1f;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -_settings.MaxLookAngle, _settings.MaxLookAngle);
            
            float targetZ = (-mouseX * _settings.SwayAmount) + (-moveInput.x * _settings.TiltAmount);
            targetZ = Mathf.Clamp(targetZ, -15f, 15f);
            
            _zRotation = Mathf.Lerp(_zRotation, targetZ, _settings.SwaySmoothness * deltaTime);
            
            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, _zRotation);
            playerTransform.Rotate(Vector3.up * mouseX, Space.Self);
        }

        public void TriggerJumpBob() { _jumpLandBobOffset = -_settings.JumpBobAmount; }
        public void TriggerLandingBob() { _jumpLandBobOffset = -_settings.LandBobAmount; }

        public void ApplyCameraBob(bool isGrounded, bool isCrouching, Transform cameraTransform, float deltaTime)
        {
            if (cameraTransform == null) return;

            _jumpLandBobOffset = Mathf.Lerp(_jumpLandBobOffset, 0f, deltaTime * _settings.JumpBobSmoothness);
            
            Vector3 localVel = cameraTransform.InverseTransformDirection(_currentVelocity);
            float speed = new Vector2(localVel.x, localVel.z).magnitude;

            float heightOffset = isCrouching ? (_settings.CrouchingHeight - _settings.StandingHeight) : 0f;
            float targetBaseY = _defaultCameraY + heightOffset + _jumpLandBobOffset;

            if (isGrounded && speed > 0.1f)
            {
                _bobTimer += deltaTime * _settings.BobFrequency * (speed / _settings.WalkSpeed);
                float yPos = targetBaseY + Mathf.Sin(_bobTimer) * _settings.BobAmplitude * (isCrouching ? 0.5f : 1f);
                cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, Mathf.Lerp(cameraTransform.localPosition.y, yPos, deltaTime * 15f), cameraTransform.localPosition.z);
            }
            else
            {
                _bobTimer = 0f;
                cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, Mathf.Lerp(cameraTransform.localPosition.y, targetBaseY, deltaTime * 15f), cameraTransform.localPosition.z);
            }
        }
    }
}
