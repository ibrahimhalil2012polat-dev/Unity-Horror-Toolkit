using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HorrorToolkit.Tests
{
    public sealed class FlashlightTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name);
            objects.Add(go);
            return go;
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            field.SetValue(target, value);
        }

        private FlashlightController NewFlashlight(out Light light, float initial = 1f, float life = 120f)
        {
            var root = NewObject("Player");
            root.SetActive(false);
            var lamp = NewObject("Lamp");
            lamp.transform.SetParent(root.transform);
            light = lamp.AddComponent<Light>();
            light.type = LightType.Spot;
            var controller = root.AddComponent<FlashlightController>();
            SetField(controller, "flashlight", light);
            SetField(controller, "initialBattery", initial);
            SetField(controller, "batteryLifeSeconds", life);
            root.SetActive(true);
            return controller;
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }

        [Test]
        public void ToggleUpdatesLightAndEmitsEventsOnce()
        {
            var f = NewFlashlight(out var light);
            int on = 0, off = 0;
            f.TurnedOn.AddListener(() => on++);
            f.TurnedOff.AddListener(() => off++);
            Assert.IsFalse(light.enabled);
            f.Toggle();
            Assert.IsTrue(f.IsOn);
            Assert.IsTrue(light.enabled);
            Assert.IsTrue(f.TryTurnOn());
            Assert.AreEqual(1, on);
            f.Toggle();
            f.TurnOff();
            Assert.IsFalse(f.IsOn);
            Assert.IsFalse(light.enabled);
            Assert.AreEqual(1, off);
        }

        [Test]
        public void EmptyBatteryBlocksActivationAndRechargeDoesNotAutoStart()
        {
            var f = NewFlashlight(out var light, 0.25f);
            int depleted = 0;
            f.BatteryDepleted.AddListener(() => depleted++);
            Assert.IsTrue(f.TryTurnOn());
            Assert.IsTrue(f.ConsumeBattery(0.5f));
            Assert.AreEqual(0f, f.BatteryLevel);
            Assert.IsFalse(f.IsOn);
            Assert.IsFalse(light.enabled);
            Assert.IsFalse(f.TryTurnOn());
            Assert.IsFalse(f.ConsumeBattery(0.5f));
            Assert.AreEqual(1, depleted);
            Assert.IsTrue(f.Recharge(0.5f));
            Assert.AreEqual(0.5f, f.BatteryLevel, 0.0001f);
            Assert.IsFalse(f.IsOn);
            f.RechargeFull();
            Assert.AreEqual(1f, f.BatteryLevel);
            Assert.IsTrue(f.TryTurnOn());
        }

        [UnityTest]
        public IEnumerator BatteryDrainsOnlyWhileOn()
        {
            var f = NewFlashlight(out _, 1f, 2f);
            yield return null;
            Assert.AreEqual(1f, f.BatteryLevel);
            Assert.IsTrue(f.TryTurnOn());
            float deadline = Time.realtimeSinceStartup + 3f;
            while (f.BatteryLevel >= 0.99f && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.Less(f.BatteryLevel, 1f);
            f.TurnOff();
            float remaining = f.BatteryLevel;
            yield return null;
            yield return null;
            Assert.AreEqual(remaining, f.BatteryLevel);
        }

        [Test]
        public void PickupIsNotConsumedWhenBatteryIsFull()
        {
            var f = NewFlashlight(out _);
            var go = NewObject("Battery");
            go.AddComponent<BoxCollider>().isTrigger = true;
            var pickup = go.AddComponent<BatteryPickup>();
            Assert.IsFalse(pickup.TryCollect(f));
            Assert.IsFalse(pickup.IsCollected);
            Assert.IsTrue(go.activeSelf);
            f.ConsumeBattery(0.75f);
            Assert.IsTrue(pickup.TryCollect(f));
            Assert.AreEqual(0.75f, f.BatteryLevel, 0.0001f);
            Assert.IsTrue(pickup.IsCollected);
            Assert.IsFalse(go.activeSelf);
            Assert.IsFalse(pickup.TryCollect(f));
        }

        [Test]
        public void DisablingControllerTurnsLightOff()
        {
            var f = NewFlashlight(out var light);
            f.TurnOn();
            f.enabled = false;
            Assert.IsFalse(f.IsOn);
            Assert.IsFalse(light.enabled);
            Assert.IsFalse(f.TryTurnOn());
            f.enabled = true;
            Assert.IsFalse(f.IsOn);
            Assert.IsTrue(f.TryTurnOn());
        }

        [Test]
        public void BatteryEventsAndLowBatteryStateUseClampedValues()
        {
            var f = NewFlashlight(out _, 0.5f);
            var values = new List<float>();
            f.BatteryChanged.AddListener(v => values.Add(v));
            Assert.IsFalse(f.ConsumeBattery(float.NaN));
            Assert.IsFalse(f.Recharge(-1f));
            Assert.IsTrue(f.ConsumeBattery(0.4f));
            Assert.IsTrue(f.IsLowBattery);
            Assert.IsTrue(f.Recharge(5f));
            Assert.IsFalse(f.IsLowBattery);
            Assert.IsFalse(f.Recharge(1f));
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(0.1f, values[0], 0.0001f);
            Assert.AreEqual(1f, values[1]);
        }
    }
}
