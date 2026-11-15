using System;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    /// <summary>Plays a bounded scare presentation using a dedicated visual and audio source.</summary>
    [DisallowMultipleComponent]
    public sealed class JumpscareController : MonoBehaviour
    {
        [SerializeField] private GameObject visual;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip scareClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.5f;
        [SerializeField, Min(0.01f)] private float duration = 1f;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField] private UnityEvent onStarted = new UnityEvent();
        [SerializeField] private UnityEvent onFinished = new UnityEvent();

        private bool initialized;
        private float remaining;
        public bool IsPlaying { get; private set; }
        public UnityEvent Started => onStarted;
        public UnityEvent Finished => onFinished;

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            if (IsValidVisual()) visual.SetActive(false);
        }

        // Never deactivate the controller itself or an ancestor.
        private bool IsValidVisual() =>
            visual != null && visual != gameObject && !transform.IsChildOf(visual.transform);

        private void Update()
        {
            if (!IsPlaying) return;
            remaining -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (remaining <= 0f) StopScare();
        }

        public void PlayScare() => TryPlayScare();

        public bool TryPlayScare()
        {
            Initialize();
            if (!isActiveAndEnabled || IsPlaying) return false;
            bool hasVisual = IsValidVisual();
            bool hasAudio = audioSource != null && audioSource.isActiveAndEnabled && scareClip != null;
            if (!hasVisual && !hasAudio) return false;

            remaining = float.IsNaN(duration) || float.IsInfinity(duration) ? 1f : Mathf.Max(0.01f, duration);
            IsPlaying = true;
            if (hasVisual) visual.SetActive(true);
            if (hasAudio && IsPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = scareClip;
                audioSource.volume = Mathf.Clamp01(volume);
                audioSource.Play();
            }
            if (IsPlaying) onStarted.Invoke();
            return true;
        }

        public void StopScare()
        {
            if (!IsPlaying) return;
            ResetPresentation();
            onFinished.Invoke();
        }

        private void ResetPresentation()
        {
            IsPlaying = false;
            remaining = 0f;
            if (audioSource != null) audioSource.Stop();
            if (IsValidVisual()) visual.SetActive(false);
        }

        private void OnDisable()
        {
            if (IsPlaying) ResetPresentation();
        }
    }
}
