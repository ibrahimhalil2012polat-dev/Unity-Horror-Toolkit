using UnityEngine;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class DoorController : MonoBehaviour
    {
        [SerializeField] private Vector3 localAxis = Vector3.up;
        [SerializeField, Range(-180f, 180f)] private float openAngle = 90f;
        [SerializeField, Min(1f)] private float degreesPerSecond = 120f;
        [SerializeField] private bool startOpen;
        private Quaternion closedRotation;
        private Quaternion targetRotation;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            closedRotation = transform.localRotation;
            SetOpen(startOpen);
            transform.localRotation = targetRotation;
        }
        private void Update()
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation,
                targetRotation, Mathf.Max(1f, degreesPerSecond) * Time.deltaTime);
        }
        public void Toggle() => SetOpen(!IsOpen);
        public void Open() => SetOpen(true);
        public void Close() => SetOpen(false);
        public void SetOpen(bool open)
        {
            IsOpen = open;
            Vector3 axis = localAxis.sqrMagnitude > 0.0001f ? localAxis.normalized : Vector3.up;
            targetRotation = closedRotation * Quaternion.AngleAxis(open ? openAngle : 0f, axis);
        }
    }
}
