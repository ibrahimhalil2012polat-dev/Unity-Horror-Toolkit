using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HorrorToolkit.Tests
{
    public sealed class ToolkitTests
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

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in objects)
                if (go != null) Object.DestroyImmediate(go);
            objects.Clear();
        }

        [Test]
        public void DoorLocksAndConsumesMatchingKeyOnlyOnce()
        {
            var door = NewObject("Door").AddComponent<DoorController>();
            var inventory = NewObject("Player").AddComponent<KeyInventory>();
            SetField(door, "requiredKeyId", "basement");
            SetField(door, "consumeKey", true);
            int opened = 0, blocked = 0;
            door.Opened.AddListener(() => opened++);
            door.Blocked.AddListener(() => blocked++);
            Assert.IsTrue(door.TryLock());
            inventory.AddKey("other");
            door.Interact(inventory);
            Assert.IsTrue(door.IsLocked);
            Assert.IsFalse(door.IsOpen);
            Assert.AreEqual(1, blocked);

            Assert.IsTrue(inventory.AddKey(" basement "));
            door.Interact(inventory);
            Assert.IsFalse(door.IsLocked);
            Assert.IsTrue(door.IsOpen);
            Assert.IsFalse(inventory.HasKey("basement"));
            Assert.IsTrue(inventory.HasKey("other"));
            Assert.AreEqual(1, opened);
            door.Open();
            Assert.AreEqual(1, opened);
            Assert.IsFalse(door.TryLock());
            door.Close();
            Assert.IsTrue(door.TryLock());
        }

        [Test]
        public void KeysAreUniqueAndCaseSensitive()
        {
            var inventory = NewObject("Inventory").AddComponent<KeyInventory>();
            Assert.IsFalse(inventory.AddKey(" "));
            Assert.IsTrue(inventory.AddKey(" a "));
            Assert.IsFalse(inventory.AddKey("a"));
            Assert.IsFalse(inventory.HasKey("A"));
            Assert.IsTrue(inventory.RemoveKey("a"));
            Assert.IsFalse(inventory.HasKey("a"));
        }

        [UnityTest]
        public IEnumerator OutageRestoresOriginalLightsAfterDelay()
        {
            var on = NewObject("On").AddComponent<Light>();
            var off = NewObject("Off").AddComponent<Light>();
            off.enabled = false;
            var outage = NewObject("Outage").AddComponent<PowerOutage>();
            SetField(outage, "lights", new[] { on, off, on });
            SetField(outage, "duration", 0.05f);
            outage.BeginOutage();
            Assert.IsTrue(outage.IsPowerOff);
            Assert.IsFalse(on.enabled);
            Assert.IsFalse(off.enabled);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (outage.IsPowerOff && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsFalse(outage.IsPowerOff);
            Assert.IsTrue(on.enabled);
            Assert.IsFalse(off.enabled);
        }

        [Test]
        public void DisablingOutageRestoresLights()
        {
            var light = NewObject("Light").AddComponent<Light>();
            var outage = NewObject("Outage").AddComponent<PowerOutage>();
            SetField(outage, "lights", new[] { light });
            outage.BeginOutage();
            Assert.IsFalse(light.enabled);
            outage.enabled = false;
            Assert.IsTrue(light.enabled);
            Assert.IsFalse(outage.IsPowerOff);
        }

        [Test]
        public void TriggerFiltersPlayerAndCanBeReset()
        {
            var outage = NewObject("Outage").AddComponent<PowerOutage>();
            var zone = NewObject("Zone");
            zone.AddComponent<BoxCollider>().isTrigger = true;
            var trigger = zone.AddComponent<PowerOutageTrigger>();
            SetField(trigger, "outage", outage);
            var stranger = NewObject("Stranger").AddComponent<BoxCollider>();
            Assert.IsFalse(trigger.TryTrigger(stranger));

            var player = NewObject("Player");
            player.tag = "Player";
            var child = NewObject("Child");
            child.transform.SetParent(player.transform);
            var collider = child.AddComponent<BoxCollider>();
            Assert.IsTrue(trigger.TryTrigger(collider));
            Assert.IsTrue(outage.IsPowerOff);
            outage.RestorePower();
            Assert.IsFalse(trigger.TryTrigger(collider));
            trigger.ResetTrigger();
            Assert.IsTrue(trigger.TryTrigger(collider));
        }
    }
}
