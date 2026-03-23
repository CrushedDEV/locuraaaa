using UnityEngine;
using UnityEngine.InputSystem;
using ScapeRoom.Interaction;

namespace ScapeRoom.Puzzles.UVLight
{
    [RequireComponent(typeof(UVLightController))]
    public class PickableUVFlashlight : BaseInteractable
    {
        [Header("Flashlight Settings")]
        [SerializeField] private Transform _equipTransform;
        [SerializeField] private Vector3 _equipLocalPosition;
        [SerializeField] private Vector3 _equipLocalRotation;
        
        [Header("Aiming Settings")]
        [Tooltip("Si se asigna, la linterna apuntará siempre hacia donde mire esta cámara")]
        [SerializeField] private Transform _playerCamera;
        [SerializeField] private bool _followCameraAim = true;
        
        [Header("Sway & Weight")]
        [Tooltip("Hace que la linterna tenga un 'retraso/peso' cuando giras la cámara")]
        [SerializeField] private bool _enableSway = true;
        [SerializeField] private float _swaySmoothness = 12f;
        
        [Header("Visuals (Optional)")]
        [SerializeField] private Light _unitySpotLight;

        private UVLightController _uvLightController;
        private bool _isEquipped = false;
        private Rigidbody _rb;
        private Collider _col;

        protected override void Awake()
        {
            base.Awake();
            _uvLightController = GetComponent<UVLightController>();
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();

            SyncLights();
        }

        private void LateUpdate()
        {
            if (_isEquipped && _followCameraAim && _playerCamera != null)
            {
                Vector3 targetPoint = _playerCamera.position + _playerCamera.forward * 100f;
                
                // Calculamos exactamente la rotación ideal
                Quaternion idealRotation = Quaternion.LookRotation(targetPoint - transform.position);
                idealRotation *= Quaternion.Euler(_equipLocalRotation);
                
                if (_enableSway)
                {
                    // Al interpolar suavemente, la linterna se quedará un poco atrás al mover la cabeza
                    // creando un efecto de peso y balanceo realista.
                    transform.rotation = Quaternion.Slerp(transform.rotation, idealRotation, Time.deltaTime * _swaySmoothness);
                }
                else
                {
                    // Modo instantáneo antiguo
                    transform.rotation = idealRotation;
                }
            }
        }

        public override void Interact()
        {
            if (!_isEquipped)
            {
                Equip();
            }
        }

        private void Equip()
        {
            if (_equipTransform == null) return;

            _isEquipped = true;
            
            // Desactivar físicas al equipar
            if (_rb != null) _rb.isKinematic = true;
            if (_col != null) _col.enabled = false;

            transform.SetParent(_equipTransform);
            transform.localPosition = _equipLocalPosition;
            if (!_followCameraAim || _playerCamera == null)
            {
                transform.localRotation = Quaternion.Euler(_equipLocalRotation);
            }
        }

        public void Drop()
        {
            if (!_isEquipped) return;

            _isEquipped = false;
            
            // Reactivar físicas al soltar
            if (_rb != null) _rb.isKinematic = false;
            if (_col != null) _col.enabled = true;

            transform.SetParent(null);
            // La linterna se queda en el entorno. Si estaba encendida, se quedará iluminando el suelo, ideal para puzles.
        }

        public void OnDropFlashlight(InputValue value)
        {
            if (value.isPressed)
            {
                Drop();
            }
        }

        // Action compatible con Unity New Input System o Unity Events
        public void ToggleLight()
        {
            if (!_isEquipped) return;
            
            _uvLightController.SetState(!_uvLightController.IsOn);
            SyncLights();
        }

        public void OnToggleFlashlight(InputValue value)
        {
            if (value.isPressed)
            {
                ToggleLight();
            }
        }

        private void SyncLights()
        {
            if (_unitySpotLight != null)
            {
                _unitySpotLight.enabled = _uvLightController.IsOn;
            }
        }
    }
}
