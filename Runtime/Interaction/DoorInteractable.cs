using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class DoorInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private DoorController door;
        [SerializeField] private string openPrompt = "Open door";
        [SerializeField] private string closePrompt = "Close door";
        [SerializeField] private string lockedPrompt = "Unlock door";

        public string Prompt
        {
            get
            {
                if (door == null) return string.Empty;
                return door.IsLocked ? lockedPrompt : door.IsOpen ? closePrompt : openPrompt;
            }
        }

        private void Awake()
        {
            if (door == null) door = GetComponentInParent<DoorController>();
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            return door != null && door.isActiveAndEnabled;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;
            door.Interact(interactor != null ? interactor.Inventory : null);
        }
    }
}
