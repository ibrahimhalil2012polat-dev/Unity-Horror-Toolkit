using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace HorrorToolkit.Tests
{
    public sealed class FootstepNoiseTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();

        private GameObject NewObject(string name)
        {
            var value = new GameObject(name);
            objects.Add(value);
            return value;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, fieldName);
            field.SetValue(target, value);
        }

        private EnemyBrain NewEnemy()
        {
            var go = NewObject("Enemy");
            go.SetActive(false);
            var agent = go.AddComponent<NavMeshAgent>();
            agent.enabled = false; // Decisions can be tested without a baked NavMesh.
            go.AddComponent<EnemyVision>();
            var brain = go.AddComponent<EnemyBrain>();
            go.SetActive(true);
            return brain;
        }

        private FootstepNoiseEmitter NewEmitter(params EnemyBrain[] enemies)
        {
            var go = NewObject("Player");
            var emitter = go.AddComponent<FootstepNoiseEmitter>();
            SetField(emitter, "listeners", enemies);
            return emitter;
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = objects.Count - 1; i >= 0; --i)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }

        [Test]
        public void RunningCanAlertEnemyOutsideWalkingHearingRadius()
        {
            var enemy = NewEnemy();
            var emitter = NewEmitter(enemy);
            emitter.transform.position = Vector3.forward * 6f;

            Assert.AreEqual(0, emitter.EmitFootstep(false));
            Assert.AreEqual(EnemyState.Patrol, enemy.State);
            Assert.AreEqual(1, emitter.EmitFootstep(true));
            Assert.AreEqual(EnemyState.Investigate, enemy.State);
            Assert.AreEqual(emitter.transform.position, enemy.LastKnownPosition);
        }

        [Test]
        public void StationaryMovementDoesNotGenerateNoise()
        {
            var emitter = NewEmitter();
            int count = 0;
            emitter.WalkingStep.AddListener(() => count++);

            emitter.TickMovement(Vector3.zero, 5f, true);
            emitter.TickMovement(Vector3.zero, 5f, true);
            Assert.AreEqual(0, count);

            emitter.TickMovement(Vector3.forward * 0.4f, 0.3f, true);
            emitter.TickMovement(Vector3.forward * 0.8f, 0.3f, true);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void FastMotionUsesRunningInterval()
        {
            var emitter = NewEmitter();
            int walks = 0, runs = 0;
            emitter.WalkingStep.AddListener(() => walks++);
            emitter.RunningStep.AddListener(() => runs++);

            emitter.TickMovement(Vector3.forward * 1.1f, 0.25f, true);
            emitter.TickMovement(Vector3.forward * 2.2f, 0.25f, true);
            Assert.AreEqual(0, walks);
            Assert.AreEqual(1, runs);
        }

        [Test]
        public void AirborneAndTeleportMovementAreIgnored()
        {
            var emitter = NewEmitter();
            int count = 0;
            emitter.WalkingStep.AddListener(() => count++);
            emitter.RunningStep.AddListener(() => count++);

            emitter.TickMovement(Vector3.forward, 0.3f, false);
            emitter.TickMovement(Vector3.forward * 2f, 0.3f, false);
            emitter.TickMovement(Vector3.forward * 100f, 0.1f, true);
            Assert.AreEqual(0, count);
        }

        [Test]
        public void PausingResetsPartialStepProgress()
        {
            var emitter = NewEmitter();
            int count = 0;
            emitter.WalkingStep.AddListener(() => count++);

            emitter.TickMovement(Vector3.forward * 0.4f, 0.3f, true);
            emitter.TickMovement(Vector3.forward * 0.4f, 2f, true);
            emitter.TickMovement(Vector3.forward * 0.8f, 0.3f, true);
            Assert.AreEqual(0, count);
            emitter.TickMovement(Vector3.forward * 1.2f, 0.3f, true);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void DisabledEmitterDoesNotSendFootsteps()
        {
            var enemy = NewEnemy();
            var emitter = NewEmitter(enemy);
            emitter.enabled = false;
            Assert.AreEqual(0, emitter.EmitFootstep(true));
            Assert.AreEqual(EnemyState.Patrol, enemy.State);
            emitter.enabled = true;
            Assert.AreEqual(1, emitter.EmitFootstep(true));
        }
    }
}
