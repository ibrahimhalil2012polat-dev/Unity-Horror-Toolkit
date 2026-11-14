using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class BatteryPickup : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float chargeAmount = 0.5f;
        private bool collected;

        public bool IsCollected => collected;

        private void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            TryCollect(other.GetComponentInParent<FlashlightController>());
        }

        /// <summary>Collects only if the flashlight accepts additional charge.</summary>
        public bool TryCollect(FlashlightController controller)
        {
            if (!isActiveAndEnabled || collected || controller == null) return false;
            // Guard against reentrant collection from a BatteryChanged listener.
            collected = true;
            if (!controller.Recharge(chargeAmount))
            {
                collected = false;
                return false;
            }
            gameObject.SetActive(false);
            return true;
        }
    }
}
