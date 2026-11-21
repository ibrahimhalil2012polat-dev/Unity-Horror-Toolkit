using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class InspectableInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Inspect";
        [SerializeField, TextArea(2, 6)] private string description = "Nothing unusual.";
        [SerializeField] private bool oneShot;
        [SerializeField] private UnityEvent<string> onInspected = new UnityEvent<string>();

        private bool inspected;

        public string Prompt => prompt;
        public bool WasInspected => inspected;
        public UnityEvent<string> Inspected => onInspected;

        public bool CanInteract(PlayerInteractor interactor)
        {
            return isActiveAndEnabled && (!oneShot || !inspected);
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;
            inspected = true;
            onInspected.Invoke(description);
        }

        public void ResetInspection() => inspected = false;
    }
}
