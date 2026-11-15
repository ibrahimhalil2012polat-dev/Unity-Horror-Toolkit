using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HorrorToolkit.Tests
{
    public sealed class ScareAudioTests
    {
        private readonly List<Object> objects = new List<Object>();

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name);
            objects.Add(go);
            return go;
        }

        private AudioClip NewClip(string name)
        {
            var clip = AudioClip.Create(name, 4410, 1, 44100, false);
            objects.Add(clip);
            return clip;
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            field.SetValue(target, value);
        }

        private JumpscareController NewScare(out GameObject visual, float duration = 1f)
        {
            var root = NewObject("Scare");
            root.SetActive(false);
            visual = NewObject("Visual");
            visual.transform.SetParent(root.transform);
            var scare = root.AddComponent<JumpscareController>();
            SetField(scare, "visual", visual);
            SetField(scare, "duration", duration);
            SetField(scare, "useUnscaledTime", true);
            root.SetActive(true);
            return scare;
        }

        [TearDown]
        public void Cleanup()
        {
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            objects.Clear();
        }

        [Test]
        public void ScareStartsOnceAndCanBeStoppedAndReplayed()
        {
            var scare = NewScare(out var visual);
            int starts = 0, finishes = 0;
            scare.Started.AddListener(() => starts++);
            scare.Finished.AddListener(() => finishes++);
            Assert.IsFalse(visual.activeSelf);
            Assert.IsTrue(scare.TryPlayScare());
            Assert.IsTrue(visual.activeSelf);
            Assert.IsFalse(scare.TryPlayScare());
            Assert.AreEqual(1, starts);
            scare.StopScare();
            scare.StopScare();
            Assert.IsFalse(visual.activeSelf);
            Assert.AreEqual(1, finishes);
            Assert.IsTrue(scare.TryPlayScare());
            Assert.AreEqual(2, starts);
        }

        [UnityTest]
        public IEnumerator ScareFinishesAfterDuration()
        {
            var scare = NewScare(out var visual, 0.05f);
            Assert.IsTrue(scare.TryPlayScare());
            float deadline = Time.realtimeSinceStartup + 3f;
            while (scare.IsPlaying && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.IsFalse(scare.IsPlaying);
            Assert.IsFalse(visual.activeSelf);
        }

        [Test]
        public void DisablingScareCleansUpVisual()
        {
            var scare = NewScare(out var visual);
            Assert.IsTrue(scare.TryPlayScare());
            scare.enabled = false;
            Assert.IsFalse(visual.activeSelf);
            Assert.IsFalse(scare.IsPlaying);
            Assert.IsFalse(scare.TryPlayScare());
        }

        [Test]
        public void TriggerUsesPlayerParentTagAndOneShotReset()
        {
            var scare = NewScare(out _);
            var zone = NewObject("Zone");
            zone.AddComponent<BoxCollider>().isTrigger = true;
            var trigger = zone.AddComponent<JumpscareTrigger>();
            SetField(trigger, "scare", scare);
            var stranger = NewObject("Stranger").AddComponent<BoxCollider>();
            Assert.IsFalse(trigger.TryTrigger(stranger));
            var player = NewObject("Player");
            player.tag = "Player";
            var child = NewObject("Collider");
            child.transform.SetParent(player.transform);
            var collider = child.AddComponent<BoxCollider>();
            Assert.IsTrue(trigger.TryTrigger(collider));
            Assert.IsTrue(trigger.HasTriggered);
            scare.StopScare();
            Assert.IsFalse(trigger.TryTrigger(collider));
            trigger.ResetTrigger();
            Assert.IsTrue(trigger.TryTrigger(collider));
        }

        [Test]
        public void ClipSelectionSkipsNullsAndAvoidsImmediateRepeat()
        {
            var a = NewClip("A");
            var b = NewClip("B");
            Assert.IsNull(RandomAmbiencePlayer.ChooseClip(null, null));
            Assert.IsNull(RandomAmbiencePlayer.ChooseClip(new AudioClip[] { null }, null));
            Assert.AreSame(b, RandomAmbiencePlayer.ChooseClip(new[] { null, a, a, b }, a));
            Assert.AreSame(a, RandomAmbiencePlayer.ChooseClip(new[] { a, null }, a));
        }

        [Test]
        public void AmbienceCanStopAndRestartWithoutClips()
        {
            var go = NewObject("Ambience");
            var player = go.AddComponent<RandomAmbiencePlayer>();
            Assert.IsTrue(player.IsRunning);
            Assert.IsFalse(player.TryPlayNext());
            player.StopAmbience();
            Assert.IsFalse(player.IsRunning);
            player.StartAmbience();
            Assert.IsTrue(player.IsRunning);
            player.enabled = false;
            Assert.IsFalse(player.IsRunning);
            player.StartAmbience();
            Assert.IsFalse(player.IsRunning);
        }
    }
}
