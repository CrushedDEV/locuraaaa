using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScapeRoom.Interaction
{
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _interactionDistance = 3f;
        [SerializeField] private LayerMask _interactableMask;

        [Header("UI")]
        [SerializeField] private Image _holdProgressBar;

        private IInteractable _currentInteractable;
        private bool _isInteractPressed;
        private float _currentHoldTimer;
        private bool _hasPerformedActionThisHold;

        private void Awake()
        {
            if (_holdProgressBar != null)
            {
                _holdProgressBar.fillAmount = 0f;
                _holdProgressBar.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            CheckForInteractable();
            ProcessHoldInteraction();
        }

        // Detect objects every frame
        private void CheckForInteractable()
        {
            if (_cameraTransform == null) return;

            Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactableMask))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if (_currentInteractable != interactable)
                    {
                        ClearInteractable(); 
                        _currentInteractable = interactable;
                        _currentInteractable.OnHoverEnter();
                    }
                }
                else
                {
                    ClearInteractable();
                }
            }
            else
            {
                ClearInteractable();
            }
        }

        // Manage click and hold logic
        private void ProcessHoldInteraction()
        {
            if (_currentInteractable == null) return;

            if (!_currentInteractable.IsHoldInteract)
            {
                if (_isInteractPressed && !_hasPerformedActionThisHold)
                {
                    _currentInteractable.Interact();
                    _hasPerformedActionThisHold = true;
                }
                return;
            }

            if (_isInteractPressed && !_hasPerformedActionThisHold)
            {
                _currentHoldTimer += Time.deltaTime;

                if (_holdProgressBar != null)
                {
                    _holdProgressBar.gameObject.SetActive(true);
                    _holdProgressBar.fillAmount = _currentHoldTimer / _currentInteractable.HoldDuration;
                }

                if (_currentHoldTimer >= _currentInteractable.HoldDuration)
                {
                    _currentInteractable.Interact();
                    _hasPerformedActionThisHold = true; 
                    ResetHoldUI();
                }
            }
            else
            {
                _currentHoldTimer = 0f;
                ResetHoldUI();
            }
        }

        private void ClearInteractable()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnHoverExit();
                _currentInteractable = null;
            }

            _currentHoldTimer = 0f;
            ResetHoldUI();
        }

        private void ResetHoldUI()
        {
            if (_holdProgressBar != null)
            {
                _holdProgressBar.fillAmount = 0f;
                _holdProgressBar.gameObject.SetActive(false);
            }
        }

        // Input Event
        public void OnInteract(InputValue value)
        {
            _isInteractPressed = value.isPressed;

            if (!_isInteractPressed)
                _hasPerformedActionThisHold = false; 
        }
    }
}
