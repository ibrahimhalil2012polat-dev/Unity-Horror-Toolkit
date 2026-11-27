using System;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class RaycastInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private KeyInventory inventory;
        [SerializeField, Min(0f)] private float reach = 3f;
        [SerializeField] private LayerMask raycastMask = Physics.DefaultRaycastLayers;
        [SerializeField] private bool useLegacyInput = true;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private UnityEvent<string> onPromptChanged = new UnityEvent<string>();
        private HorrorInteractable focused;
        private string lastPrompt = string.Empty;

        public HorrorInteractable Focused => focused;
        public UnityEvent<string> PromptChanged => onPromptChanged;

        private void Update()
        {
            RefreshFocus();
#if ENABLE_LEGACY_INPUT_MANAGER
            if (useLegacyInput && Input.GetKeyDown(interactionKey)) TryInteract();
#endif
        }

        private void OnDisable() => SetFocus(null);

        public HorrorInteractable RefreshFocus()
        {
            SetFocus(isActiveAndEnabled ? FindTarget() : null);
            return focused;
        }

        // Void entry point for UI buttons and input actions.
        public void Interact() => TryInteract();

        public bool TryInteract()
        {
            var target = RefreshFocus();
            if (target == null) return false;
            var root = playerRoot != null ? playerRoot : transform;
            var keys = inventory != null ? inventory : root.GetComponentInParent<KeyInventory>();
            bool result = target.TryInteract(keys);
            RefreshFocus();
            return result;
        }

        private HorrorInteractable FindTarget()
        {
            if (viewCamera == null || reach <= 0f) return null;
            var root = playerRoot != null ? playerRoot : transform;
            var hits = Physics.RaycastAll(viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)),
                reach, raycastMask, QueryTriggerInteraction.Collide);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                var collider = hit.collider;
                if (collider == null || collider.transform.IsChildOf(root)) continue;
                var candidate = collider.GetComponentInParent<HorrorInteractable>();
                if (candidate != null && candidate.IsAvailable) return candidate;
                // Unrelated trigger volumes do not obstruct interaction; solid objects do.
                if (!collider.isTrigger) return null;
            }
            return null;
        }

        private void SetFocus(HorrorInteractable target)
        {
            focused = target;
            string nextPrompt = target != null ? target.Prompt : string.Empty;
            if (nextPrompt == lastPrompt) return;
            lastPrompt = nextPrompt;
            onPromptChanged.Invoke(nextPrompt);
        }
    }
}
