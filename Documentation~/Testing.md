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

The five test cases cover key consumption, inventory uniqueness, delayed light restoration, restoration on disable, and trigger tag filtering/reset. These tests are authored but have not yet been run in Unity.

## Manual checks

- Install in a clean Unity project and check the console for compilation errors.
- Door: toggle repeatedly; test negative angles and initial locked/open states; verify physical rotation and events.
- Key: set up a player inventory and trigger pickup; collect a key, unlock matching door and check duplicate behavior.
- Outage: assign initially enabled and disabled lights; verify early restoration, zero duration and disable mid-outage.
- Trigger: enter with a player child collider; check one-shot mode and rearm. Use a trigger collider and Rigidbody.
- LAN: install dependencies, import sample, configure NetworkManager/player prefab, connect two instances, disconnect and reconnect. Test two devices on the same LAN.

No Unity Editor test or build results have been recorded yet.
