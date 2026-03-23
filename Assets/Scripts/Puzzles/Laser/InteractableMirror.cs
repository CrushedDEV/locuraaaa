using UnityEngine;
using System.Collections;
using ScapeRoom.Interaction;

namespace ScapeRoom.Puzzles
{
    [RequireComponent(typeof(LaserMirror))]
    public class InteractableMirror : BaseInteractable
    {
        [Header("Rotation Settings")]
        [SerializeField] private float _rotationStep = 45f;
        [SerializeField] private float _rotationSpeed = 8f;

        private float _targetYRotation;
        private Coroutine _rotationCoroutine;

        protected override void Awake()
        {
            base.Awake(); 
            _targetYRotation = transform.localEulerAngles.y;
        }

        public override void Interact()
        {
            _targetYRotation += _rotationStep;
            
            if (_rotationSpeed <= 0.1f)
            {
                // Instant rotation
                transform.localRotation = Quaternion.Euler(
                    transform.localEulerAngles.x, 
                    _targetYRotation, 
                    transform.localEulerAngles.z
                );
            }
            else
            {
                // Animated rotation
                if (_rotationCoroutine != null) StopCoroutine(_rotationCoroutine);
                _rotationCoroutine = StartCoroutine(RotateSmoothlyCoroutine());
            }
        }

        private IEnumerator RotateSmoothlyCoroutine()
        {
            Quaternion startRot = transform.localRotation;
            Quaternion endRot = Quaternion.Euler(transform.localEulerAngles.x, _targetYRotation, transform.localEulerAngles.z);
            
            float progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime * _rotationSpeed;
                transform.localRotation = Quaternion.Slerp(startRot, endRot, progress);
                yield return null;
            }

            transform.localRotation = endRot;
        }
    }
}
