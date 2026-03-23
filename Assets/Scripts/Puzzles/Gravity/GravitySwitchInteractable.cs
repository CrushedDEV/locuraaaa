using UnityEngine;
using ScapeRoom.Interaction;
using ScapeRoom.Player;

namespace ScapeRoom.Puzzles.Gravity
{
    public class GravitySwitchInteractable : BaseInteractable
    {
        [Header("Gravity Elements")]
        [Tooltip("Si dejas esto vacío, el script buscará automáticamente al PlayerController en la escena al iniciar.")]
        [SerializeField] private PlayerController _playerController;

        protected override void Awake()
        {
            base.Awake();
            if (_playerController == null)
            {
                _playerController = FindFirstObjectByType<PlayerController>();
            }
        }

        public override void Interact()
        {
            if (_playerController != null)
            {
                _playerController.FlipGravity();
            }
            else
            {
                // Si por algún motivo no hay jugador (modo test de cajas), invierte al menos el mundo.
                Physics.gravity = -Physics.gravity;
            }
        }
    }
}
