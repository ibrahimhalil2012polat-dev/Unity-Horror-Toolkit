using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace HorrorToolkit.Tests
{
    public sealed class InteractionTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();

        private GameObject NewObject(string name, float z = 0f)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(0f, 0f, z);
            objects.Add(go);
            return go;
        }

        private static void SetField(object obj, string name, object value)
        {
            var field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            field.SetValue(obj, value);
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in objects)
                if (go != null) Object.DestroyImmediate(go);
            objects.Clear();
        }

        private RaycastInteractor Player()
        {
            var root = NewObject("Player");
            var camera = root.AddComponent<Camera>();
            var interactor = root.AddComponent<RaycastInteractor>();
            SetField(interactor, "viewCamera", camera);
            return interactor;
        }

        [Test]
        public void OneShotFiresOnceAndCanBeReset()
        {
            var target = NewObject("Switch").AddComponent<HorrorInteractable>();
            SetField(target, "oneShot", true);
            int count = 0;
            target.Interacted.AddListener(() => count++);
            Assert.IsTrue(target.TryInteract(null));
            Assert.IsFalse(target.TryInteract(null));
            target.ResetInteraction();
            Assert.IsTrue(target.TryInteract(null));
            Assert.AreEqual(2, count);
        }

        [Test]
        public void RaycastRejectsWallsAndOutOfReachTargets()
        {
            var player = Player();
            var go = NewObject("Switch", 2f);
            go.AddComponent<BoxCollider>();
            var target = go.AddComponent<HorrorInteractable>();
            var wall = NewObject("Wall", 1f);
            wall.AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            Assert.IsNull(player.RefreshFocus());
            Assert.IsFalse(player.TryInteract());
            wall.SetActive(false);
            Physics.SyncTransforms();
            Assert.AreEqual(target, player.RefreshFocus());
            go.transform.position = new Vector3(0f, 0f, 10f);
            Physics.SyncTransforms();
            Assert.IsFalse(player.TryInteract());
        }

        [Test]
        public void RaycastIgnoresPlayerCollidersAndUnrelatedTriggers()
        {
            var player = Player();
            var self = NewObject("SelfCollider", 0.8f);
            self.transform.SetParent(player.transform, true);
            self.AddComponent<BoxCollider>();
            var zone = NewObject("Zone", 1.5f);
            zone.AddComponent<BoxCollider>().isTrigger = true;
            var go = NewObject("Switch", 2.7f);
            go.AddComponent<BoxCollider>();
            var target = go.AddComponent<HorrorInteractable>();
            int count = 0;
            target.Interacted.AddListener(() => count++);
            Physics.SyncTransforms();
            Assert.AreEqual(target, player.RefreshFocus());
            Assert.IsTrue(player.TryInteract());
            Assert.AreEqual(1, count);
        }

        [Test]
        public void DoorInteractionRequiresMatchingKeyWhenLocked()
        {
            var go = NewObject("Door");
            var door = go.AddComponent<DoorController>();
            var target = go.AddComponent<HorrorInteractable>();
            var inventory = NewObject("Player").AddComponent<KeyInventory>();
            SetField(target, "door", door);
            SetField(target, "oneShot", true);
            door.TryLock();
            Assert.IsFalse(target.TryInteract(inventory));
            Assert.IsTrue(target.IsAvailable);
            inventory.AddKey("door-key");
            Assert.IsTrue(target.TryInteract(inventory));
            Assert.IsTrue(door.IsOpen);
            Assert.IsFalse(target.IsAvailable);
        }

        [Test]
        public void KeyPickupUsesInventoryAndClearsFocus()
        {
            var player = Player();
            var keys = player.gameObject.AddComponent<KeyInventory>();
            var go = NewObject("Key", 2f);
            go.AddComponent<BoxCollider>().isTrigger = true;
            var pickup = go.AddComponent<KeyPickup>();
            var target = go.AddComponent<HorrorInteractable>();
            SetField(target, "keyPickup", pickup);
            var prompts = new List<string>();
            player.PromptChanged.AddListener(value => prompts.Add(value));
            Physics.SyncTransforms();
            Assert.IsTrue(player.TryInteract());
            Assert.IsTrue(keys.HasKey("door-key"));
            Assert.IsFalse(go.activeSelf);
            Assert.IsNull(player.Focused);
            CollectionAssert.AreEqual(new[] { "Interact", "" }, prompts);
        }
    }
}
