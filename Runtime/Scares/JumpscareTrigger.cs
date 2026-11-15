using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class JumpscareTrigger : MonoBehaviour
    {
        [SerializeField] private JumpscareController scare;
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool oneShot = true;
        private bool triggered;
        private bool starting;

        public bool HasTriggered => triggered;
        private void OnTriggerEnter(Collider other) => TryTrigger(other);

        public bool TryTrigger(Collider other)
        {
            if (!isActiveAndEnabled || other == null || scare == null || starting ||
                (oneShot && triggered) || !MatchesTag(other.transform)) return false;

            starting = true;
            try
            {
                if (!scare.TryPlayScare()) return false;
                triggered = true;
                return true;
            }
            finally { starting = false; }
        }

        public void ResetTrigger() => triggered = false;

        private bool MatchesTag(Transform candidate)
        {
            if (string.IsNullOrWhiteSpace(requiredTag)) return true;
            string expected = requiredTag.Trim();
            while (candidate != null)
            {
                if (candidate.gameObject.tag == expected) return true;
                candidate = candidate.parent;
            }
            return false;
        }
    }
}
