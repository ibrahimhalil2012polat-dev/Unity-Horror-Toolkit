using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    /// <summary>
    /// Reports player footsteps to explicitly assigned EnemyBrain listeners.
    /// Can estimate footsteps from movement or receive animation-event calls.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FootstepNoiseEmitter : MonoBehaviour
    {
        [SerializeField] private EnemyBrain[] listeners;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private bool automaticDetection = true;
        [SerializeField] private bool groundedOnly = true;
        [SerializeField, Min(0f)] private float minimumMoveSpeed = 0.2f;
        [SerializeField, Min(0.01f)] private float runningSpeed = 3.5f;
        [SerializeField, Min(0.1f)] private float maximumMoveSpeed = 12f;
        [SerializeField, Min(0.05f)] private float walkingStepInterval = 0.55f;
        [SerializeField, Min(0.05f)] private float runningStepInterval = 0.35f;
        [SerializeField, Range(0f, 1f)] private float walkingLoudness = 0.4f;
        [SerializeField, Range(0f, 1f)] private float runningLoudness = 1f;
        [SerializeField] private UnityEvent onWalkingStep = new UnityEvent();
        [SerializeField] private UnityEvent onRunningStep = new UnityEvent();

        private Vector3 previousPosition;
        private float elapsedSinceStep;

        public UnityEvent WalkingStep => onWalkingStep;
        public UnityEvent RunningStep => onRunningStep;

        private void OnEnable() => ResetTracking();

        /// <summary>Clears accumulated movement, for example after teleporting the player.</summary>
        public void ResetTracking()
        {
            previousPosition = transform.position;
            elapsedSinceStep = 0f;
        }

        private void LateUpdate()
        {
            if (!automaticDetection)
            {
                previousPosition = transform.position;
                return;
            }

            bool isGrounded = !groundedOnly || characterController == null ||
                characterController.isGrounded;
            TickMovement(transform.position, Time.deltaTime, isGrounded);
        }

        /// <summary>
        /// Processes one movement sample. Intended for deterministic tests or custom
        /// movement controllers when Automatic Detection is disabled.
        /// </summary>
        public void TickMovement(Vector3 worldPosition, float deltaTime, bool isGrounded)
        {
            if (!isActiveAndEnabled) return;

            Vector3 delta = worldPosition - previousPosition;
            previousPosition = worldPosition;

            if (!IsFinite(deltaTime) || deltaTime <= 0f ||
                !IsFinite(delta.x) || !IsFinite(delta.y) || !IsFinite(delta.z))
            {
                elapsedSinceStep = 0f;
                return;
            }

            float planarSpeed = new Vector2(delta.x, delta.z).magnitude / deltaTime;
            if (!isGrounded || !IsFinite(planarSpeed) ||
                planarSpeed < Mathf.Max(0f, minimumMoveSpeed) ||
                planarSpeed > Mathf.Max(0.1f, maximumMoveSpeed))
            {
                elapsedSinceStep = 0f;
                return;
            }

            bool running = planarSpeed >= Mathf.Max(0.01f, runningSpeed);
            float interval = Mathf.Max(0.05f,
                running ? runningStepInterval : walkingStepInterval);

            elapsedSinceStep += deltaTime;
            if (elapsedSinceStep < interval) return;

            // Avoid a burst of queued noises after frame stalls or a pause.
            elapsedSinceStep = 0f;
            EmitAt(worldPosition, running);
        }

        /// <summary>Call from an animation event to report one walking footstep.</summary>
        public void EmitWalkingFootstep() => EmitFootstep(false);

        /// <summary>Call from an animation event to report one running footstep.</summary>
        public void EmitRunningFootstep() => EmitFootstep(true);

        /// <summary>Returns the number of enemy listeners that accepted the noise.</summary>
        public int EmitFootstep(bool running)
        {
            if (!isActiveAndEnabled) return 0;
            return EmitAt(transform.position, running);
        }

        private int EmitAt(Vector3 worldPosition, bool running)
        {
            float loudness = Mathf.Clamp01(running ? runningLoudness : walkingLoudness);
            if (!IsFinite(loudness) || loudness <= 0f) return 0;

            if (running) onRunningStep.Invoke();
            else onWalkingStep.Invoke();

            int alerted = 0;
            if (listeners == null) return alerted;
            for (int i = 0; i < listeners.Length; i++)
            {
                var enemy = listeners[i];
                if (enemy != null && enemy.ReportNoise(worldPosition, loudness))
                    alerted++;
            }
            return alerted;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
