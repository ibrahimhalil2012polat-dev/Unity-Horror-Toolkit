using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class KeyPickup : MonoBehaviour
    {
        [SerializeField] private string keyId = "door-key";
        [SerializeField] private bool collectOnTrigger = true;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField, Range(0f, 1f)] private float pickupVolume = 1f;
        [SerializeField] private UnityEvent<string> onCollected = new UnityEvent<string>();
        private bool collected;

        public bool IsCollected => collected;
        public UnityEvent<string> Collected => onCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (!collectOnTrigger || other == null) return;
            TryCollect(other.GetComponentInParent<KeyInventory>());
        }

        // Also callable from a raycast, input action or other interaction system.
        public bool TryCollect(KeyInventory inventory)
        {
            if (!isActiveAndEnabled || collected || inventory == null ||
                !inventory.AddKey(keyId)) return false;
            collected = true;
            if (pickupClip != null)
                AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);
            onCollected.Invoke(keyId.Trim());
            gameObject.SetActive(false);
            return true;
        }
    }
}
