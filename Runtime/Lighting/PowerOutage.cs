using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class PowerOutage : MonoBehaviour
    {
        [SerializeField] private Light[] lights;
        [SerializeField, Min(0f)] private float duration = 10f;
        [SerializeField] private UnityEvent onOutageStarted = new UnityEvent();
        [SerializeField] private UnityEvent onPowerRestored = new UnityEvent();
        private readonly Dictionary<Light, bool> originalStates = new Dictionary<Light, bool>();
        private Coroutine timer;
        private int generation;
        public bool IsPowerOff { get; private set; }

        public void BeginOutage()
        {
            if (!isActiveAndEnabled || IsPowerOff) return;
            int current = ++generation;
            originalStates.Clear();
            if (lights != null)
                foreach (Light light in lights)
                {
                    if (light == null || originalStates.ContainsKey(light)) continue;
                    originalStates.Add(light, light.enabled);
                    light.enabled = false;
                }
            IsPowerOff = true;
            onOutageStarted.Invoke();
            if (generation != current || !IsPowerOff || !isActiveAndEnabled) return;
            if (duration <= 0f) RestorePower();
            else timer = StartCoroutine(RestoreAfterDelay());
        }
        private IEnumerator RestoreAfterDelay()
        {
            yield return new WaitForSeconds(duration);
            timer = null;
            RestorePower();
        }
        public void RestorePower()
        {
            if (timer != null) StopCoroutine(timer);
            timer = null;
            if (!IsPowerOff) return;
            RestoreLights();
            onPowerRestored.Invoke();
        }
        private void RestoreLights()
        {
            ++generation;
            foreach (var entry in originalStates)
                if (entry.Key != null) entry.Key.enabled = entry.Value;
            originalStates.Clear();
            IsPowerOff = false;
        }
        private void OnDisable()
        {
            if (timer != null) StopCoroutine(timer);
            timer = null;
            if (IsPowerOff) RestoreLights();
        }
    }
}
