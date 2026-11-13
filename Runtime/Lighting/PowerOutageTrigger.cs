using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PowerOutageTrigger : MonoBehaviour
    {
        [SerializeField] private PowerOutage outage;
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool oneShot = true;
        private bool triggered;

        public bool HasTriggered => triggered;

        private void OnTriggerEnter(Collider other) => TryTrigger(other);

        public bool TryTrigger(Collider other)
        {
            if (!isActiveAndEnabled || other == null || outage == null ||
                !outage.isActiveAndEnabled || outage.IsPowerOff || (oneShot && triggered))
                return false;

            if (!MatchesTag(other.transform)) return false;
            // Set before invoking events, preventing reentrant one-shot activation.
            triggered = true;
            outage.BeginOutage();
            return true;
        }

        public void ResetTrigger() => triggered = false;

        private bool MatchesTag(Transform candidate)
        {
            if (string.IsNullOrWhiteSpace(requiredTag)) return true;
            string tagToMatch = requiredTag.Trim();
            while (candidate != null)
            {
                if (candidate.gameObject.tag == tagToMatch) return true;
                candidate = candidate.parent;
            }
            return false;
        }
    }
}
