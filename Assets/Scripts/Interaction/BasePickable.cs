using UnityEngine;

namespace ScapeRoom.Interaction
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public abstract class BasePickable : BaseInteractable, IPickable
    {
        [Header("Pickable Settings")]
        [SerializeField] protected Vector3 _equipLocalPosition = Vector3.zero;
        [SerializeField] protected Vector3 _equipLocalRotation = Vector3.zero;

        protected bool _isEquipped = false;
        protected Rigidbody _rb;
        protected Collider _col;

        public bool IsEquipped => _isEquipped;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<Collider>();
        }

        public override void Interact()
        {
            if (!_isEquipped)
            {
                // Let the EquipmentController handle the actual PickUp call
                // so we can invoke an event or notify a manager if needed.
                // However, the cleanest way without tight coupling is:
                // The interaction system just invokes Interact().
                // If the player is the one interacting, the PlayerInteractor 
                // could notify the EquipmentController.
                // For simplicity and decoupling, we can let the player's 
                // EquipmentController listen to interactions, OR have the item 
                // find the player's equipment controller. Let's find the equipment controller.
                
                var equipmentController = FindObjectOfType<ScapeRoom.Player.EquipmentController>();
                if (equipmentController != null)
                {
                    equipmentController.EquipItem(this);
                }
                else
                {
                    Debug.LogWarning("EquipmentController not found in the scene.");
                }
            }
        }

        public virtual void PickUp(Transform equipTransform)
        {
            if (equipTransform == null) return;

            _isEquipped = true;
            
            if (_rb != null) _rb.isKinematic = true;
            if (_col != null) _col.enabled = false;

            transform.SetParent(equipTransform);
            transform.localPosition = _equipLocalPosition;
            transform.localRotation = Quaternion.Euler(_equipLocalRotation);
        }

        public virtual void Drop()
        {
            if (!_isEquipped) return;

            _isEquipped = false;
            
            if (_rb != null) _rb.isKinematic = false;
            if (_col != null) _col.enabled = true;

            transform.SetParent(null);
        }
    }
}
