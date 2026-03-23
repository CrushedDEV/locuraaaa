using UnityEngine;
using UnityEngine.Events;

namespace ScapeRoom.Puzzles
{
    [RequireComponent(typeof(Collider))]
    public class LaserSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [SerializeField] private float _timeToActivate = 0f;
        
        [Header("Events")]
        public UnityEvent OnLaserEnterEvent;
        public UnityEvent OnLaserActivatedEvent;
        public UnityEvent OnLaserExitEvent;

        private float _currentChargeTime;
        private bool _isFullyActivated;

        public void OnLaserHit()
        {
            OnLaserEnterEvent?.Invoke();
        }

        public void OnLaserStay(float deltaTime)
        {
            if (!_isFullyActivated)
            {
                _currentChargeTime += deltaTime;

                if (_currentChargeTime >= _timeToActivate)
                {
                    _isFullyActivated = true;
                    OnLaserActivatedEvent?.Invoke();
                }
            }
        }

        public void OnLaserLost()
        {
            _currentChargeTime = 0f;
            _isFullyActivated = false;
            OnLaserExitEvent?.Invoke();
        }
    }
}
