using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class KeyPickup : MonoBehaviour
    {
        [SerializeField] private string keyId = "door-key";
        [SerializeField] private bool collectOnTrigger = true;
        private bool collected;

        public string KeyId => keyId;
        public bool IsCollected => collected;

        private void OnTriggerEnter(Collider other)
        {
            if (!collectOnTrigger || other == null) return;
            TryCollect(other.GetComponentInParent<KeyInventory>());
        }

        /// <summary>Collects the key on explicit player interaction or trigger entry.</summary>
        public bool TryCollect(KeyInventory inventory)
        {
            if (!isActiveAndEnabled || collected || inventory == null ||
                !inventory.AddKey(keyId)) return false;

            collected = true;
            gameObject.SetActive(false);
            return true;
        }
    }
}
