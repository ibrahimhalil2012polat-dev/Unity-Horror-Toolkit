using UnityEngine;

namespace HorrorToolkit
{
    /// <summary>Range, field-of-view and obstacle checks for a single target.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyVision : MonoBehaviour
    {
        [SerializeField] private Transform eye;
        [SerializeField, Min(0f)] private float eyeHeight = 1.5f;
        [SerializeField, Min(0f)] private float targetHeight = 1f;
        [SerializeField, Min(0f)] private float viewDistance = 12f;
        [SerializeField, Range(0f, 360f)] private float fieldOfView = 100f;
        [Tooltip("Only environment obstacles. Exclude the enemy and target colliders.")]
        [SerializeField] private LayerMask obstacleMask;

        public bool CanSee(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy ||
                !isActiveAndEnabled) return false;

            Vector3 origin = eye != null ? eye.position : transform.position + Vector3.up * eyeHeight;
            Vector3 forward = eye != null ? eye.forward : transform.forward;
            Vector3 offset = target.position + Vector3.up * targetHeight - origin;
            float distance = offset.magnitude;
            if (distance > Mathf.Max(0f, viewDistance)) return false;
            if (distance <= 0.001f) return true;
            Vector3 direction = offset / distance;
            if (Vector3.Angle(forward, direction) > Mathf.Clamp(fieldOfView, 0f, 360f) * 0.5f)
                return false;

            return !Physics.Raycast(origin, direction, distance, obstacleMask, QueryTriggerInteraction.Ignore);
        }
    }
}
