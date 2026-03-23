using UnityEngine;
using UnityEngine.Events;

namespace ScapeRoom.Interaction
{
    public abstract class BaseInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected bool _isHoldInteract = false;
        [SerializeField] protected float _holdDuration = 1.0f;

        public bool IsHoldInteract => _isHoldInteract;
        public float HoldDuration => _holdDuration;

        private const string HIGHLIGHT_LAYER_NAME = "Outline";
        
        private int _originalLayer;
        private int _highlightLayerId;

        [Header("Events")]
        [SerializeField] protected UnityEvent _onHoverStartEvent;
        [SerializeField] protected UnityEvent _onHoverEndEvent;

        protected virtual void Awake()
        {
            _originalLayer = gameObject.layer;
            _highlightLayerId = LayerMask.NameToLayer(HIGHLIGHT_LAYER_NAME);
        }

        public virtual void OnHoverEnter()
        {
            if (_highlightLayerId > -1)
                gameObject.layer = _highlightLayerId;

            _onHoverStartEvent?.Invoke();
        }

        public virtual void OnHoverExit()
        {
            gameObject.layer = _originalLayer;
            _onHoverEndEvent?.Invoke();
        }

        public abstract void Interact();
    }
}
