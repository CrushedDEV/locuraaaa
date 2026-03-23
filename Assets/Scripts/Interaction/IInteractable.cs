namespace ScapeRoom.Interaction
{
    public interface IInteractable
    {
        bool IsHoldInteract { get; }
        float HoldDuration { get; }

        void OnHoverEnter();
        void OnHoverExit();
        void Interact();
    }
}
