using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace HorrorToolkit
{
    public enum EnemyState { Patrol, Chase, Investigate, Search }

    /// <summary>Single-target NPC behaviour driven by sight and explicitly reported game noises.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent), typeof(EnemyVision))]
    public sealed class EnemyBrain : MonoBehaviour
    {
        [Serializable]
        public sealed class StateEvent : UnityEvent<EnemyState> { }

        [SerializeField] private Transform target;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField, Min(0f)] private float patrolSpeed = 2f;
        [SerializeField, Min(0f)] private float chaseSpeed = 4f;
        [SerializeField, Min(0f)] private float investigationSpeed = 2.5f;
        [SerializeField, Min(0f)] private float hearingDistance = 10f;
        [SerializeField, Min(0f)] private float investigationDuration = 8f;
        [SerializeField, Min(0f)] private float searchDuration = 5f;
        [SerializeField, Min(0.05f)] private float repathInterval = 0.2f;
        [SerializeField, Min(0.05f)] private float arrivalTolerance = 0.5f;
        [SerializeField] private StateEvent onStateChanged = new StateEvent();

        private NavMeshAgent agent;
        private EnemyVision vision;
        private Vector3 destination;
        private Vector3 lastKnown;
        private Vector3 lastRequested;
        private bool hasRequested;
        private float stateRemaining;
        private float repathRemaining;
        private int patrolIndex;

        public EnemyState State { get; private set; } = EnemyState.Patrol;
        public Vector3 LastKnownPosition => lastKnown;
        public StateEvent StateChanged => onStateChanged;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            vision = GetComponent<EnemyVision>();
        }

        public void SetTarget(Transform value) => target = value;

        private void Update() => Tick(Time.deltaTime);

        // Exposed for deterministic simulation and tests. Do not also call from another Update.
        public void Tick(float deltaTime)
        {
            if (!isActiveAndEnabled || !IsFinite(deltaTime) || deltaTime < 0f) return;
            repathRemaining = Mathf.Max(0f, repathRemaining - deltaTime);

            if (vision != null && vision.CanSee(target))
            {
                lastKnown = target.position;
                destination = lastKnown;
                ChangeState(EnemyState.Chase);
                MoveTo(destination, chaseSpeed);
                return;
            }

            if (State == EnemyState.Chase)
            {
                BeginSearch(lastKnown);
            }
            else if (State == EnemyState.Investigate || State == EnemyState.Search)
            {
                stateRemaining -= deltaTime;
                if (State == EnemyState.Investigate &&
                    (stateRemaining <= 0f || Reached(destination)))
                    BeginSearch(destination);
                else if (State == EnemyState.Search && stateRemaining <= 0f)
                    ChangeState(EnemyState.Patrol);
            }

            switch (State)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Investigate:
                case EnemyState.Search:
                    if (Reached(destination)) StopMoving();
                    else MoveTo(destination, investigationSpeed);
                    break;
            }
        }

        /// <summary>Reports a game-world sound. Loudness scales the hearing radius.</summary>
        public bool ReportNoise(Vector3 position, float loudness = 1f)
        {
            if (!isActiveAndEnabled || State == EnemyState.Chase ||
                !IsFinite(position.x) || !IsFinite(position.y) || !IsFinite(position.z) ||
                !IsFinite(loudness) || loudness <= 0f) return false;

            float radius = Mathf.Max(0f, hearingDistance) * Mathf.Clamp01(loudness);
            if (Vector3.Distance(transform.position, position) > radius) return false;
            destination = position;
            lastKnown = position;
            stateRemaining = Mathf.Max(0f, investigationDuration);
            repathRemaining = 0f;
            ChangeState(EnemyState.Investigate);
            return true;
        }

        private void BeginSearch(Vector3 position)
        {
            destination = position;
            stateRemaining = Mathf.Max(0f, searchDuration);
            ChangeState(EnemyState.Search);
        }

        private void ChangeState(EnemyState next)
        {
            if (State == next) return;
            State = next;
            repathRemaining = 0f;
            hasRequested = false;
            onStateChanged.Invoke(next);
        }

        private void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                StopMoving();
                return;
            }

            // Bounded scan: skip missing waypoints and advance past reached ones.
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Transform point = patrolPoints[patrolIndex];
                if (point != null && !Reached(point.position))
                {
                    MoveTo(point.position, patrolSpeed);
                    return;
                }
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }
            StopMoving();
        }

        private bool Reached(Vector3 position)
        {
            float tolerance = Mathf.Max(0.05f, arrivalTolerance);
            if (CanNavigate()) tolerance = Mathf.Max(tolerance, agent.stoppingDistance);
            // Ground-plane distance, so a waypoint at floor level works with an agent base offset.
            Vector3 offset = position - transform.position;
            return offset.x * offset.x + offset.z * offset.z <= tolerance * tolerance &&
                Mathf.Abs(offset.y) <= Mathf.Max(tolerance, 1f);
        }

        private bool CanNavigate() => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;

        private void MoveTo(Vector3 position, float speed)
        {
            if (!CanNavigate()) return;
            agent.speed = Mathf.Max(0f, speed);
            agent.isStopped = false;
            if (repathRemaining > 0f || agent.pathPending) return;
            if (hasRequested && (lastRequested - position).sqrMagnitude < 0.01f &&
                agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete) return;
            hasRequested = agent.SetDestination(position);
            lastRequested = position;
            repathRemaining = Mathf.Max(0.05f, repathInterval);
        }

        private void StopMoving()
        {
            hasRequested = false;
            if (CanNavigate())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        private void OnDisable() => StopMoving();

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
