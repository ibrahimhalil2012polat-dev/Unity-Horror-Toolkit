using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private Vector3 localAxis = Vector3.up;
        [SerializeField, Range(-180f, 180f)] private float openAngle = 90f;
        [SerializeField, Min(1f)] private float degreesPerSecond = 120f;
        [SerializeField] private bool startOpen;
        [SerializeField] private bool startLocked;
        [SerializeField] private string requiredKeyId = "door-key";
        [SerializeField] private bool consumeKey;
        [SerializeField] private UnityEvent onOpened = new UnityEvent();
        [SerializeField] private UnityEvent onClosed = new UnityEvent();
        [SerializeField] private UnityEvent onLocked = new UnityEvent();
        [SerializeField] private UnityEvent onUnlocked = new UnityEvent();
        [SerializeField] private UnityEvent onBlocked = new UnityEvent();

        [Header("Optional audio and feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip unlockClip;
        [SerializeField] private AudioClip blockedClip;
        [SerializeField] private UnityEvent<string> onMissingKey = new UnityEvent<string>();

        public UnityEvent<string> MissingKey => onMissingKey;

        private Quaternion closedRotation;
        private Quaternion targetRotation;
        private bool initialized;
        public bool IsOpen { get; private set; }
        public bool IsLocked { get; private set; }
        public UnityEvent Opened => onOpened;
        public UnityEvent Closed => onClosed;
        public UnityEvent Locked => onLocked;
        public UnityEvent Unlocked => onUnlocked;
        public UnityEvent Blocked => onBlocked;

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            closedRotation = transform.localRotation;
            IsLocked = startLocked;
            IsOpen = startOpen && !startLocked;
            SetTarget();
            transform.localRotation = targetRotation;
        }

        private void Update()
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation,
                targetRotation, Mathf.Max(1f, degreesPerSecond) * Time.deltaTime);
        }

        public void Toggle() { Initialize(); SetOpen(!IsOpen); }
        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);

        public void SetOpen(bool open)
        {
            Initialize();
            if (open && IsLocked) { ReportBlocked(); return; }
            if (IsOpen == open) return;
            IsOpen = open;
            SetTarget();
            if (open) onOpened.Invoke();
            else onClosed.Invoke();
        }

        private void SetTarget()
        {
            Vector3 axis = localAxis.sqrMagnitude > 0.0001f ? localAxis.normalized : Vector3.up;
            targetRotation = closedRotation * Quaternion.AngleAxis(IsOpen ? openAngle : 0f, axis);
        }

        public void Lock() => TryLock();
        public bool TryLock()
        {
            Initialize();
            if (IsOpen) return false;
            if (IsLocked) return true;
            IsLocked = true;
            onLocked.Invoke();
            return true;
        }

        // Explicit scripted override, e.g. a puzzle or remote switch.
        public void Unlock()
        {
            Initialize();
            if (!IsLocked) return;
            IsLocked = false;
            PlayFeedback(unlockClip);
            onUnlocked.Invoke();
        }

        public bool TryUnlock(KeyInventory inventory)
        {
            Initialize();
            if (!IsLocked) return true;
            if (inventory == null || !inventory.HasKey(requiredKeyId)) return false;
            if (consumeKey && !inventory.RemoveKey(requiredKeyId)) return false;
            Unlock();
            return true;
        }

        private void PlayFeedback(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void ReportBlocked()
        {
            PlayFeedback(blockedClip);
            onBlocked.Invoke();
            onMissingKey.Invoke(requiredKeyId == null ? string.Empty : requiredKeyId.Trim());
        }

        public void Interact(KeyInventory inventory)
        {
            Initialize();
            if (IsLocked && !TryUnlock(inventory))
            {
                ReportBlocked();
                return;
            }
            Toggle();
        }
    }
}
