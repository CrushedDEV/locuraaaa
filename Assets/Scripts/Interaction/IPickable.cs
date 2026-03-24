using UnityEngine;

namespace ScapeRoom.Interaction
{
    public interface IPickable
    {
        void PickUp(Transform equipTransform);
        void Drop();
        bool IsEquipped { get; }
    }
}
