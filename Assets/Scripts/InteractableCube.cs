using UnityEngine;
using ScapeRoom.Interaction;

public class InteractableCube : BaseInteractable
{

    public override void Interact()
    {
        this.gameObject.SetActive(false);
    }
}
