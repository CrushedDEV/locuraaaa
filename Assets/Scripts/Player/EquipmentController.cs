using UnityEngine;
using UnityEngine.InputSystem;
using ScapeRoom.Interaction;

namespace ScapeRoom.Player
{
    public class EquipmentController : MonoBehaviour
    {
        [Header("Equipment Settings")]
        [SerializeField] private Transform _equipTransform;

        private IPickable _currentEquipment;

        public bool HasEquipment => _currentEquipment != null;

        public void EquipItem(IPickable item)
        {
            if (item == null) return;

            // Drop current item if we have one
            if (_currentEquipment != null)
            {
                DropCurrentItem();
            }

            _currentEquipment = item;
            _currentEquipment.PickUp(_equipTransform);
        }

        public void DropCurrentItem()
        {
            if (_currentEquipment != null && _currentEquipment.IsEquipped)
            {
                _currentEquipment.Drop();
                _currentEquipment = null;
            }
        }

        // --- Input Handling ---

        public void OnDrop(InputValue value)
        {
            if (value.isPressed)
            {
                DropCurrentItem();
            }
        }

        public void OnUse(InputValue value)
        {
            if (value.isPressed && _currentEquipment != null)
            {
                if (_currentEquipment is IUsable usableItem)
                {
                    usableItem.Use();
                }
            }
        }
    }
}
