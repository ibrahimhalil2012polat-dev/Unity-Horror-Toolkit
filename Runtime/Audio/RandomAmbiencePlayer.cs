using UnityEngine;

namespace HorrorToolkit
{
    /// <summary>Schedules non-overlapping clips at random intervals using a dedicated AudioSource.</summary>
    [DisallowMultipleComponent]
    public sealed class RandomAmbiencePlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] clips;
        [SerializeField, Min(0.1f)] private float minimumDelay = 5f;
        [SerializeField, Min(0.1f)] private float maximumDelay = 15f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField] private bool playAutomatically = true;
        [SerializeField] private bool useUnscaledTime;

        private bool running;
        private float remaining;
        private AudioClip previous;
        public bool IsRunning => running;
        public AudioClip LastClip => previous;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (playAutomatically) StartAmbience();
        }

        private void Update()
        {
            if (!running) return;
            remaining -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (remaining > 0f) return;
            if (audioSource != null && audioSource.isPlaying) return;
            TryPlayNext();
            ScheduleNext();
        }

        public void StartAmbience()
        {
            if (!isActiveAndEnabled || running) return;
            running = true;
            ScheduleNext();
        }

        public void StopAmbience()
        {
            running = false;
            if (audioSource != null) audioSource.Stop();
        }

        public void PlayNext() => TryPlayNext();

        public bool TryPlayNext()
        {
            if (!isActiveAndEnabled || audioSource == null ||
                !audioSource.isActiveAndEnabled || audioSource.isPlaying) return false;

            AudioClip next = ChooseClip(clips, previous);
            if (next == null) return false;
            audioSource.loop = false;
            audioSource.clip = next;
            audioSource.volume = Mathf.Clamp01(volume);
            audioSource.Play();
            previous = next;
            ScheduleNext();
            return true;
        }

        // Reservoir sampling selects among non-null alternatives without allocations.
        // A repeated reference is allowed only if there is no different clip.
        public static AudioClip ChooseClip(AudioClip[] candidates, AudioClip last)
        {
            if (candidates == null) return null;
            AudioClip choice = null;
            AudioClip fallback = null;
            int seen = 0;
            foreach (AudioClip candidate in candidates)
            {
                if (candidate == null) continue;
                fallback = candidate;
                if (candidate == last) continue;
                seen++;
                if (Random.Range(0, seen) == 0) choice = candidate;
            }
            return choice != null ? choice : fallback;
        }

        private void ScheduleNext()
        {
            float a = ValidDelay(minimumDelay, 5f);
            float b = ValidDelay(maximumDelay, 15f);
            remaining = Random.Range(Mathf.Min(a, b), Mathf.Max(a, b));
        }

        private static float ValidDelay(float value, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Max(0.1f, value);

        private void OnDisable() => StopAmbience();
    }
}
