using UnityEngine;
using ScapeRoom.Interaction;

namespace ScapeRoom.Puzzles.UVLight
{
    [RequireComponent(typeof(UVLightController))]
    public class PickableUVFlashlight : BasePickable, IUsable
    {
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

        protected override void Awake()
        {
            base.Awake();
            _uvLightController = GetComponent<UVLightController>();

            // Auto-assign the camera so the flashlight always aims at screen centre
            if (_playerCamera == null && Camera.main != null)
                _playerCamera = Camera.main.transform;

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

        public void Use()
        {
            if (!_isEquipped) return;
            
            _uvLightController.SetState(!_uvLightController.IsOn);
            SyncLights();
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
