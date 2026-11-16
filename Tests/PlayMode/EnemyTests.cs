using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorToolkit.Tests
{
    public sealed class EnemyTests
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

        private EnemyBrain NewEnemy(out EnemyVision vision)
        {
            var go = NewObject("Enemy");
            go.SetActive(false);
            var agent = go.AddComponent<NavMeshAgent>();
            agent.enabled = false; // Test sensing and decisions without baked navigation.
            vision = go.AddComponent<EnemyVision>();
            var brain = go.AddComponent<EnemyBrain>();
            SetField(vision, "eyeHeight", 0f);
            SetField(vision, "targetHeight", 0f);
            go.SetActive(true);
            return brain;
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }

        [Test]
        public void VisionRespectsDistanceAngleAndTargetActivity()
        {
            NewEnemy(out var vision);
            var target = NewObject("Target");
            target.transform.position = Vector3.forward * 5f;
            Assert.IsTrue(vision.CanSee(target.transform));
            target.transform.position = Vector3.back * 5f;
            Assert.IsFalse(vision.CanSee(target.transform));
            target.transform.position = Vector3.forward * 20f;
            Assert.IsFalse(vision.CanSee(target.transform));
            target.transform.position = Vector3.forward * 5f;
            target.SetActive(false);
            Assert.IsFalse(vision.CanSee(target.transform));
            Assert.IsFalse(vision.CanSee(null));
        }

        [Test]
        public void WallBlocksVisionButTriggerDoesNot()
        {
            NewEnemy(out var vision);
            SetField(vision, "obstacleMask", (LayerMask)(1 << 0));
            var target = NewObject("Target");
            target.transform.position = Vector3.forward * 5f;
            var wall = NewObject("Wall");
            wall.transform.position = Vector3.forward * 2.5f;
            var box = wall.AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            Assert.IsFalse(vision.CanSee(target.transform));
            box.isTrigger = true;
            Physics.SyncTransforms();
            Assert.IsTrue(vision.CanSee(target.transform));
        }

        [Test]
        public void LosingSightSearchesLastKnownPositionThenPatrols()
        {
            var brain = NewEnemy(out _);
            var target = NewObject("Target");
            target.transform.position = Vector3.forward * 5f;
            brain.SetTarget(target.transform);
            SetField(brain, "searchDuration", 1f);
            var states = new List<EnemyState>();
            brain.StateChanged.AddListener(s => states.Add(s));

            brain.Tick(0f);
            Assert.AreEqual(EnemyState.Chase, brain.State);
            Vector3 last = target.transform.position;
            target.transform.position = Vector3.back * 5f;
            brain.Tick(0f);
            Assert.AreEqual(EnemyState.Search, brain.State);
            Assert.AreEqual(last, brain.LastKnownPosition);
            brain.Tick(1.1f);
            Assert.AreEqual(EnemyState.Patrol, brain.State);
            CollectionAssert.AreEqual(new[] { EnemyState.Chase, EnemyState.Search, EnemyState.Patrol }, states);
        }

        [Test]
        public void NoiseUsesHearingRadiusAndExpires()
        {
            var brain = NewEnemy(out _);
            SetField(brain, "investigationDuration", 1f);
            SetField(brain, "searchDuration", 1f);
            Assert.IsFalse(brain.ReportNoise(Vector3.forward * 8f, 0.5f));
            Assert.IsFalse(brain.ReportNoise(Vector3.zero, float.NaN));
            Assert.IsFalse(brain.ReportNoise(Vector3.zero, 0f));
            Assert.IsTrue(brain.ReportNoise(Vector3.forward * 4f, 0.5f));
            Assert.AreEqual(EnemyState.Investigate, brain.State);
            Assert.AreEqual(Vector3.forward * 4f, brain.LastKnownPosition);
            brain.Tick(1.1f);
            Assert.AreEqual(EnemyState.Search, brain.State);
            brain.Tick(1.1f);
            Assert.AreEqual(EnemyState.Patrol, brain.State);
        }

        [Test]
        public void SeeingTargetOverridesNoiseAndNoiseCannotInterruptChase()
        {
            var brain = NewEnemy(out _);
            Assert.IsTrue(brain.ReportNoise(Vector3.back * 2f));
            var target = NewObject("Target");
            target.transform.position = Vector3.forward * 3f;
            brain.SetTarget(target.transform);
            brain.Tick(0f);
            Assert.AreEqual(EnemyState.Chase, brain.State);
            Assert.IsFalse(brain.ReportNoise(Vector3.back));
            Assert.AreEqual(target.transform.position, brain.LastKnownPosition);
        }

        [Test]
        public void ArrivingAtNoiseBeginsSearchAndSightCanResumeChase()
        {
            var brain = NewEnemy(out _);
            Assert.IsTrue(brain.ReportNoise(Vector3.zero));
            brain.Tick(0f);
            Assert.AreEqual(EnemyState.Search, brain.State);
            var target = NewObject("Target");
            target.transform.position = Vector3.forward * 3f;
            brain.SetTarget(target.transform);
            brain.Tick(0f);
            Assert.AreEqual(EnemyState.Chase, brain.State);
            brain.enabled = false;
            Assert.IsFalse(brain.ReportNoise(Vector3.zero));
        }
    }
}
