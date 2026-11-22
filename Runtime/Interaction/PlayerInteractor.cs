using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private KeyInventory inventory;
        [SerializeField, Min(0.1f)] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool enableLegacyInput = true;
        [SerializeField] private UnityEvent<string> onPromptChanged = new UnityEvent<string>();

        private readonly List<MonoBehaviour> componentBuffer = new List<MonoBehaviour>();

        public IInteractable Focused { get; private set; }
        public string CurrentPrompt { get; private set; } = string.Empty;
        public UnityEvent<string> PromptChanged => onPromptChanged;
        public KeyInventory Inventory
        {
            get
            {
                if (inventory == null) inventory = GetComponentInParent<KeyInventory>();
                return inventory;
            }
        }

        private void Awake()
        {
            if (viewCamera == null) viewCamera = Camera.main;
            if (inventory == null) inventory = GetComponentInParent<KeyInventory>();
        }

        private void Update()
        {
            RefreshFocus();
#if ENABLE_LEGACY_INPUT_MANAGER
            if (enableLegacyInput && Input.GetKeyDown(interactKey))
                TryInteract();
#endif
        }

        /// <summary>Also callable from a Unity Input System action callback.</summary>
        public bool TryInteract()
        {
            RefreshFocus();
            var target = Focused;
            if (target == null || !target.CanInteract(this)) return false;

            target.Interact(this);
            RefreshFocus();
            return true;
        }

        /// <summary>Raycasts from the view camera to update the focused target and UI prompt.</summary>
        public void RefreshFocus()
        {
            var camera = viewCamera != null ? viewCamera : Camera.main;
            IInteractable target = null;

            if (camera != null &&
                Physics.Raycast(camera.transform.position, camera.transform.forward,
                    out var hit, Mathf.Max(0.1f, interactionDistance), interactionLayers,
                    triggerInteraction))
            {
                target = FindTarget(hit.collider.transform);
            }

            string label = target == null ? string.Empty : target.Prompt;
            string prompt = string.IsNullOrWhiteSpace(label)
                ? string.Empty
                : interactKey + " - " + label;

            if (!ReferenceEquals(Focused, target) || CurrentPrompt != prompt)
            {
                Focused = target;
                CurrentPrompt = prompt;
                onPromptChanged.Invoke(prompt);
            }
        }

        private IInteractable FindTarget(Transform hitTransform)
        {
            for (Transform current = hitTransform; current != null; current = current.parent)
            {
                componentBuffer.Clear();
                current.GetComponents(componentBuffer);
                for (int i = 0; i < componentBuffer.Count; i++)
                {
                    var behaviour = componentBuffer[i];
                    if (behaviour != null && behaviour.isActiveAndEnabled &&
                        behaviour is IInteractable interactable &&
                        interactable.CanInteract(this))
                        return interactable;
                }
            }
            return null;
        }

        private void OnDisable()
        {
            if (Focused == null && CurrentPrompt.Length == 0) return;
            Focused = null;
            CurrentPrompt = string.Empty;
            onPromptChanged.Invoke(string.Empty);
        }
    }
}
