# Unity Horror Toolkit

MIT-licensed reusable Unity horror-game components. Independently implemented; no private game assets or source code included.

## Features

- Animated hinge doors, configurable locks, key inventory and collectible keys.
- Door events for audio, UI and puzzle integration.
- Timed power outages that restore original light states, plus player-triggered blackout zones.
- Optional LAN multiplayer host/client sample (Netcode for GameObjects).

## Install

Targets Unity 2022.3 LTS and newer. Unity Editor compilation and runtime testing are pending. Install via Package Manager > + > Add package from git URL:

`https://github.com/ibrahimhalil2012polat-dev/Unity-Horror-Toolkit.git`

Alternatively, choose Add package from disk and select package.json in an extracted copy. The runtime has no third-party dependencies.

## Door and keys

1. Create a hinge GameObject with the door mesh as a child, initially closed. Add DoorController to the hinge.
2. Configure local axis, angle and speed. Wire a button or interaction script to Toggle(), Open() or Close().
3. For a locked door, set Start Locked and Required Key Id (for example, basement). Start Locked overrides Start Open.
4. Add KeyInventory to the player's root GameObject. Add KeyPickup to a collectible with a 3D trigger collider and matching Key Id. Ensure at least one participant in the trigger interaction has a Rigidbody (kinematic is fine).
5. Call door.Interact(playerInventory) from your player interaction script. It tries to unlock with the matching key and then toggles the door. Consume Key removes the key when unlocking, if selected.

Key IDs are trimmed and case-sensitive; inventory is in memory, with one entry per unique key. Pickups deactivate when a new key is collected. Duplicate keys are not collected. Lock() refuses to lock an open door; Unlock() is a scripted override. Opened/Closed events signal a target-state change, not completion of rotation. No collision avoidance, save/load or network synchronization is included.

## Power outage

Add PowerOutage to an active GameObject, assign lights, and set duration. Call BeginOutage() from a button or script, or add PowerOutageTrigger to a separate zone with a 3D collider marked Is Trigger. Assign the outage in the Inspector; tag the player root Player (or set another required tag). Child colliders are supported. At least one participant must have a Rigidbody. One Shot is enabled by default; call ResetTrigger() to rearm. Clear Required Tag to allow any collider.

Wire the outage start and restore events to audio or visual effects. Original enabled states are restored, including on disable. The timer uses scaled game time. Does not change baked lighting or emissive materials; avoid overlapping outages controlling the same lights.

## LAN multiplayer

Install Netcode for GameObjects and Unity Transport first (NGO 1.8+ API target), then import LAN Multiplayer from the package Samples tab. See [setup](Samples~/LanMultiplayer/README.md). This is a direct-IP LAN example, not a production online service. No room codes, relay, matchmaking, authentication, player cap or gameplay synchronization are included.

## Testing and contributions

The package includes five Play Mode test cases for doors, inventory, outages and trigger filtering. See [testing instructions](Documentation~/Testing.md) to enable and run them. Tests have been added but not executed in Unity yet. Reproducible bug reports and actual test results are welcome.

Version 0.2.0 is an early development version, not a production-ready release.

## License

[MIT](LICENSE).
