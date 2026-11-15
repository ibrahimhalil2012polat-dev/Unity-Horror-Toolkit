# Testing Unity Horror Toolkit

## Automated Play Mode tests

1. Install the package into a clean Unity 2022.3 LTS or Unity 6 project.
2. Install Unity Test Framework (com.unity.test-framework) via Package Manager if not already present.
3. In the consuming project's Packages/manifest.json, add the toolkit to the top-level testables array (preserve existing entries):

```json
"testables": ["com.clasicstudio.horror-toolkit"]
```

4. Open Window > General > Test Runner, select PlayMode, and run the toolkit tests. Use normal game time (Time.timeScale = 1).
5. Record the Unity version, Test Framework version, OS and result.

The seventeen test cases cover key consumption, inventory uniqueness, delayed light restoration, restoration on disable, trigger tag filtering/reset, flashlight toggle/events, battery depletion and recharge, time-based drain, pickup behavior, disabling the flashlight, clamped battery events, scare lifecycle and cleanup, one-shot player filtering, clip selection, and ambience scheduler lifecycle. These tests are authored but have not yet been run in Unity.

## Manual checks

- Install in a clean Unity project and check the console for compilation errors.
- Door: toggle repeatedly; test negative angles and initial locked/open states; verify physical rotation and events.
- Key: set up a player inventory and trigger pickup; collect a key, unlock matching door and check duplicate behavior.
- Outage: assign initially enabled and disabled lights; verify early restoration, zero duration and disable mid-outage.
- Trigger: enter with a player child collider; check one-shot mode and rearm. Use a trigger collider and Rigidbody.
- Flashlight: configure a Spot Light and input button, check battery depletion, recharge without auto-starting, disable/re-enable the controller, and verify UI events. Test battery collection via a trigger and Rigidbody with full and partial charge.
- Jumpscare: assign a visual and a dedicated audio source; verify duration, repeated activation, early stop, disable cleanup and trigger rearm. Test audio volume and pause behavior manually.
- Ambience: assign several clips (including null and duplicate entries), verify random timing, no overlap, repeat avoidance, stop/restart, and spatial sound in the editor.
- LAN: install dependencies, import sample, configure NetworkManager/player prefab, connect two instances, disconnect and reconnect. Test two devices on the same LAN.

No Unity Editor test or build results have been recorded yet.
