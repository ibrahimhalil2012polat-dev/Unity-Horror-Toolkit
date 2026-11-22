using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class KeyPickupInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private KeyPickup pickup;
        [SerializeField] private string prompt = "Pick up key";

        public string Prompt => prompt;

        private void Awake()
        {
            if (pickup == null) pickup = GetComponentInParent<KeyPickup>();
        }

        public bool CanInteract(PlayerInteractor interactor)
        {
            return pickup != null && pickup.isActiveAndEnabled && !pickup.IsCollected &&
                interactor != null && interactor.Inventory != null &&
                !interactor.Inventory.HasKey(pickup.KeyId);
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;
            pickup.TryCollect(interactor.Inventory);
        }
    }
}
