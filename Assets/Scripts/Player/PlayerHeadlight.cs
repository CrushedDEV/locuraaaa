using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScapeRoom.Player
{
    /// <summary>
    /// Controls an on/off headlight attached to the player's head.
    /// Fully decoupled: just needs a Light reference and reacts to the
    /// OnToggleHeadLight input event sent by PlayerInput.
    /// </summary>
    public class PlayerHeadlight : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Light component on the player's head. Assign in Inspector.")]
        [SerializeField] private Light _headLight;
        [Tooltip("If empty, Camera.main is used automatically.")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Settings")]
        [SerializeField] private bool _startOn = false;
        [SerializeField] private float lightFadeSpeed = 1.0f;

        private float _defaultIntensity;

        private void Awake()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            if (_headLight != null)
                _defaultIntensity = _headLight.intensity;
        }

        private void Start()
        {
            if (_headLight == null)
            {
                Debug.LogWarning("[PlayerHeadlight] No Light assigned. Headlight will not work.");
                return;
            }

            _headLight.enabled = _startOn;
        }

        private void LateUpdate()
        {
            if (_headLight != null && _cameraTransform != null)
                _headLight.transform.rotation = _cameraTransform.rotation;
        }

        // Called automatically by PlayerInput (Send Messages / Broadcast Messages mode)
        // Bound to the "ToggleLight" action in the Input Actions asset.
        private bool isOn = true;
        private Coroutine currentFade;

        public void OnToggleHeadLight(InputValue value)
        {
            if (_headLight == null) return;

            if (value.isPressed)
            {
                isOn = !isOn;

                // parar fade anterior si existe
                if (currentFade != null)
                {
                    StopCoroutine(currentFade);
                }

                currentFade = StartCoroutine(FadeLight(isOn));
            }
        }

        private IEnumerator FadeLight(bool turnOn)
        {
            float targetIntensity = turnOn ? _defaultIntensity : 0f;

            while (Mathf.Abs(_headLight.intensity - targetIntensity) > 0.01f)
            {
                _headLight.intensity = Mathf.MoveTowards(
                    _headLight.intensity,
                    targetIntensity,
                    Time.deltaTime * lightFadeSpeed
                );

                yield return null;
            }

            _headLight.intensity = targetIntensity;
        }
    }
}
