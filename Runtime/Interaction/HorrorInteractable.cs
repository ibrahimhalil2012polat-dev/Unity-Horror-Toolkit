using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class HorrorInteractable : MonoBehaviour
    {
        [SerializeField] private string prompt = "Interact";
        [SerializeField] private bool oneShot;
        [SerializeField] private DoorController door;
        [SerializeField] private KeyPickup keyPickup;
        [SerializeField] private UnityEvent onInteracted = new UnityEvent();
        private bool used;
        private bool busy;

        public string Prompt => prompt ?? string.Empty;
        public bool IsAvailable => isActiveAndEnabled && !busy && (!oneShot || !used)
            && (keyPickup == null || (keyPickup.isActiveAndEnabled && !keyPickup.IsCollected))
            && (door == null || door.isActiveAndEnabled)
            && !(door != null && keyPickup != null);
        public UnityEvent Interacted => onInteracted;

        public bool TryInteract(KeyInventory inventory)
        {
            if (!IsAvailable) return false;
            busy = true;
            try
            {
                if (keyPickup != null && !keyPickup.TryCollect(inventory)) return false;
                if (door != null)
                {
                    bool wasOpen = door.IsOpen;
                    door.Interact(inventory);
                    if (door == null || door.IsOpen == wasOpen) return false;
                }
                used = true;
                onInteracted.Invoke();
                return true;
            }
            finally { busy = false; }
        }

        public void ResetInteraction() => used = false;
    }
}
