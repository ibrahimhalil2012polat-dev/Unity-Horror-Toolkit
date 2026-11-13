using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class KeyPickup : MonoBehaviour
    {
        [SerializeField] private string keyId = "door-key";
        private bool collected;

        private void OnTriggerEnter(Collider other)
        {
            if (other == null || collected) return;
            var inventory = other.GetComponentInParent<KeyInventory>();
            if (inventory == null || !inventory.AddKey(keyId)) return;
            collected = true;
            gameObject.SetActive(false);
        }
    }
}
