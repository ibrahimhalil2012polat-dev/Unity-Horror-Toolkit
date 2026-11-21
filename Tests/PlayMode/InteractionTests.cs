using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace HorrorToolkit.Tests
{
    public sealed class InteractionTests
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

        private PlayerInteractor CreatePlayer(out KeyInventory keys)
        {
            var player = NewObject("Player");
            keys = player.AddComponent<KeyInventory>();
            var cameraObject = NewObject("View");
            cameraObject.transform.SetParent(player.transform);
            var camera = cameraObject.AddComponent<Camera>();
            var interactor = player.AddComponent<PlayerInteractor>();
            SetField(interactor, "viewCamera", camera);
            return interactor;
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }

        [Test]
        public void ExaminingAnObjectUpdatesPromptAndHonorsOneShot()
        {
            var interactor = CreatePlayer(out _);
            var target = NewObject("Letter");
            target.transform.position = new Vector3(0f, 0f, 2f);
            target.AddComponent<BoxCollider>();
            var inspect = target.AddComponent<InspectableInteractable>();
            SetField(inspect, "oneShot", true);
            SetField(inspect, "description", "Look behind the tower.");
            string message = null;
            inspect.Inspected.AddListener(value => message = value);

            Physics.SyncTransforms();
            interactor.RefreshFocus();
            Assert.AreEqual("E - Inspect", interactor.CurrentPrompt);
            Assert.IsTrue(interactor.TryInteract());
            Assert.AreEqual("Look behind the tower.", message);
            Assert.IsTrue(inspect.WasInspected);
            Assert.AreEqual(string.Empty, interactor.CurrentPrompt);
            Assert.IsFalse(interactor.TryInteract());
            inspect.ResetInspection();
            interactor.RefreshFocus();
            Assert.AreEqual("E - Inspect", interactor.CurrentPrompt);
        }

        [Test]
        public void InteractingWithLockedDoorUsesInventoryKey()
        {
            var interactor = CreatePlayer(out var keys);
            Assert.IsTrue(keys.AddKey("tower"));
            var target = NewObject("Door");
            target.transform.position = new Vector3(0f, 0f, 2f);
            target.AddComponent<BoxCollider>();
            var door = target.AddComponent<DoorController>();
            SetField(door, "requiredKeyId", "tower");
            SetField(door, "consumeKey", true);
            Assert.IsTrue(door.TryLock());
            target.AddComponent<DoorInteractable>();

            Physics.SyncTransforms();
            interactor.RefreshFocus();
            Assert.AreEqual("E - Unlock door", interactor.CurrentPrompt);
            Assert.IsTrue(interactor.TryInteract());
            Assert.IsFalse(door.IsLocked);
            Assert.IsTrue(door.IsOpen);
            Assert.IsFalse(keys.HasKey("tower"));
            Assert.AreEqual("E - Close door", interactor.CurrentPrompt);
        }

        [Test]
        public void KeyCanBeCollectedByLookingAndPressingInteract()
        {
            var interactor = CreatePlayer(out var keys);
            var target = NewObject("Key");
            target.transform.position = new Vector3(0f, 0f, 2f);
            target.AddComponent<BoxCollider>().isTrigger = true;
            var pickup = target.AddComponent<KeyPickup>();
            SetField(pickup, "keyId", "attic");
            SetField(pickup, "collectOnTrigger", false);
            target.AddComponent<KeyPickupInteractable>();

            Physics.SyncTransforms();
            interactor.RefreshFocus();
            Assert.AreEqual("E - Pick up key", interactor.CurrentPrompt);
            Assert.IsTrue(interactor.TryInteract());
            Assert.IsTrue(keys.HasKey("attic"));
            Assert.IsTrue(pickup.IsCollected);
            Assert.IsFalse(target.activeSelf);
            Assert.AreEqual(string.Empty, interactor.CurrentPrompt);
        }

        [Test]
        public void DuplicateKeyIsNotOfferedAgain()
        {
            var interactor = CreatePlayer(out var keys);
            keys.AddKey("attic");
            var target = NewObject("Key");
            target.transform.position = new Vector3(0f, 0f, 2f);
            target.AddComponent<BoxCollider>();
            var pickup = target.AddComponent<KeyPickup>();
            SetField(pickup, "keyId", "attic");
            target.AddComponent<KeyPickupInteractable>();

            Physics.SyncTransforms();
            interactor.RefreshFocus();
            Assert.AreEqual(string.Empty, interactor.CurrentPrompt);
            Assert.IsFalse(interactor.TryInteract());
            Assert.IsTrue(target.activeSelf);
        }

        [Test]
        public void OrdinaryColliderBlocksRaycastInteraction()
        {
            var interactor = CreatePlayer(out _);
            var target = NewObject("Letter");
            target.transform.position = new Vector3(0f, 0f, 2.5f);
            target.AddComponent<BoxCollider>();
            target.AddComponent<InspectableInteractable>();
            var wall = NewObject("Wall");
            wall.transform.position = new Vector3(0f, 0f, 1f);
            wall.AddComponent<BoxCollider>();

            Physics.SyncTransforms();
            interactor.RefreshFocus();
            Assert.AreEqual(string.Empty, interactor.CurrentPrompt);
            Assert.IsFalse(interactor.TryInteract());
        }

        [Test]
        public void MovingOutOfRangeClearsThePrompt()
        {
            var interactor = CreatePlayer(out _);
            var target = NewObject("Letter");
            target.transform.position = new Vector3(0f, 0f, 2f);
            target.AddComponent<BoxCollider>();
            target.AddComponent<InspectableInteractable>();
            var prompts = new List<string>();
            interactor.PromptChanged.AddListener(value => prompts.Add(value));

            Physics.SyncTransforms();
            interactor.RefreshFocus();
            target.transform.position = new Vector3(0f, 0f, 10f);
            Physics.SyncTransforms();
            interactor.RefreshFocus();

            CollectionAssert.AreEqual(new[] { "E - Inspect", string.Empty }, prompts);
            Assert.AreEqual(string.Empty, interactor.CurrentPrompt);
        }
    }
}
