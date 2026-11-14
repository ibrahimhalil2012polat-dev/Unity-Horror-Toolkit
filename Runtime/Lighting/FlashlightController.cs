using System;
using UnityEngine;
using UnityEngine.Events;

namespace HorrorToolkit
{
    [DisallowMultipleComponent]
    public sealed class FlashlightController : MonoBehaviour
    {
        [Serializable]
        public sealed class BatteryLevelEvent : UnityEvent<float> { }

        [SerializeField] private Light flashlight;
        [SerializeField] private bool startOn;
        [SerializeField, Range(0f, 1f)] private float initialBattery = 1f;
        [SerializeField, Min(0.01f)] private float batteryLifeSeconds = 120f;
        [SerializeField, Range(0f, 1f)] private float lowBatteryThreshold = 0.2f;
        [SerializeField] private UnityEvent onTurnedOn = new UnityEvent();
        [SerializeField] private UnityEvent onTurnedOff = new UnityEvent();
        [SerializeField] private UnityEvent onBatteryDepleted = new UnityEvent();
        [SerializeField] private BatteryLevelEvent onBatteryChanged = new BatteryLevelEvent();

        private bool initialized;
        public bool IsOn { get; private set; }
        public float BatteryLevel { get; private set; }
        public bool IsLowBattery => BatteryLevel <= Mathf.Clamp01(lowBatteryThreshold);
        public UnityEvent TurnedOn => onTurnedOn;
        public UnityEvent TurnedOff => onTurnedOff;
        public UnityEvent BatteryDepleted => onBatteryDepleted;
        public BatteryLevelEvent BatteryChanged => onBatteryChanged;

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;
            if (flashlight == null) flashlight = GetComponent<Light>();
            BatteryLevel = IsFinite(initialBattery) ? Mathf.Clamp01(initialBattery) : 1f;
            IsOn = startOn && BatteryLevel > 0f && flashlight != null && isActiveAndEnabled;
            ApplyLight();
        }

        private void Update()
        {
            if (!IsOn) return;
            if (flashlight == null) { TurnOff(); return; }
            float life = IsFinite(batteryLifeSeconds) ? Mathf.Max(0.01f, batteryLifeSeconds) : 120f;
            ConsumeBattery(Time.deltaTime / life);
        }

        public void Toggle()
        {
            Initialize();
            if (IsOn) TurnOff();
            else TryTurnOn();
        }

        public void TurnOn() => TryTurnOn();

        public bool TryTurnOn()
        {
            Initialize();
            if (!isActiveAndEnabled || flashlight == null || BatteryLevel <= 0f) return false;
            if (IsOn) return true;
            IsOn = true;
            ApplyLight();
            onTurnedOn.Invoke();
            return true;
        }

        public void TurnOff()
        {
            Initialize();
            if (!IsOn) return;
            IsOn = false;
            ApplyLight();
            onTurnedOff.Invoke();
        }

        /// <summary>Consumes a normalized fraction of a full battery.</summary>
        public bool ConsumeBattery(float amount)
        {
            Initialize();
            if (!IsFinite(amount) || amount <= 0f) return false;
            return SetBattery(BatteryLevel - amount);
        }

        /// <summary>Restores a normalized fraction; false if the charge did not increase.</summary>
        public bool Recharge(float amount)
        {
            Initialize();
            if (!IsFinite(amount) || amount <= 0f) return false;
            return SetBattery(BatteryLevel + amount);
        }

        public void RechargeFull() => Recharge(1f);

        private bool SetBattery(float value)
        {
            float next = Mathf.Clamp01(value);
            if (next == BatteryLevel) return false;
            bool depleted = next <= 0f && BatteryLevel > 0f;
            BatteryLevel = next;
            bool switchedOff = depleted && IsOn;
            if (switchedOff)
            {
                IsOn = false;
                ApplyLight();
            }
            onBatteryChanged.Invoke(next);
            if (switchedOff) onTurnedOff.Invoke();
            if (depleted) onBatteryDepleted.Invoke();
            return true;
        }

        private void ApplyLight()
        {
            if (flashlight != null) flashlight.enabled = IsOn;
        }

        private void OnDisable()
        {
            if (initialized) TurnOff();
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
