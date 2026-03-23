using UnityEngine;

namespace ScapeRoom.Puzzles.UVLight
{
    public class UVLightController : MonoBehaviour, IUVLightSource
    {
        [Header("Light Transform")]
        [Tooltip("Asigna un objeto vacío (hijo) cuya flecha AZUL apunte al frente de la linterna. Si lo dejas vacío, usará el de este objeto.")]
        [SerializeField] private Transform _lightOrigin;

        [Header("Light Settings")]
        [SerializeField] private bool _isOn = true;
        [SerializeField] private float _range = 10f;
        [SerializeField, Range(1f, 179f)] private float _spotAngle = 45f;
        
        private static readonly int UVLightPositionId = Shader.PropertyToID("_UVLightPosition");
        private static readonly int UVLightDirectionId = Shader.PropertyToID("_UVLightDirection");
        private static readonly int UVLightParamsId = Shader.PropertyToID("_UVLightParameters");

        public Vector3 Position => _lightOrigin != null ? _lightOrigin.position : transform.position;
        public Vector3 Forward => _lightOrigin != null ? _lightOrigin.forward : transform.forward;
        public bool IsOn => _isOn;
        public float Range => _range;
        public float SpotAngle => _spotAngle;

        private void Update()
        {
            UpdateGlobalShaderVariables();
        }

        private void UpdateGlobalShaderVariables()
        {
            if (!_isOn)
            {
                Shader.SetGlobalVector(UVLightParamsId, Vector4.zero);
                return;
            }

            Transform origin = _lightOrigin != null ? _lightOrigin : transform;

            Shader.SetGlobalVector(UVLightPositionId, origin.position);
            Shader.SetGlobalVector(UVLightDirectionId, origin.forward);
            
            // X = Range, Y = Cosine of Half Angle (para dot product check), Z = IsOn
            float angleCos = Mathf.Cos(_spotAngle * 0.5f * Mathf.Deg2Rad);
            Shader.SetGlobalVector(UVLightParamsId, new Vector4(_range, angleCos, 1f, 0f));
        }

        public void SetState(bool isOn)
        {
            _isOn = isOn;
            UpdateGlobalShaderVariables();
        }
    }
}
