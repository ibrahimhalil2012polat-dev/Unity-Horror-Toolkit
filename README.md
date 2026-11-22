# Unity Horror Toolkit

MIT-licensed reusable Unity horror-game components. Independently implemented; no private game assets or source code included.

## Features

- Animated hinge doors, configurable locks, key inventory and collectible keys.
- Door events for audio, UI and puzzle integration.
- Camera-raycast E interaction for doors, manual key pickup and inspectable props, with UI prompt events.
- Timed power outages that restore original light states, plus player-triggered blackout zones.
- Rechargeable flashlight with battery drain, low-battery status and collectible batteries.
- Timed jumpscare presentation with one-shot player triggers and optional sound.
- Randomized ambient audio with repeat avoidance and configurable intervals.
- NavMesh NPC with patrol, sight-based chase, explicit noise investigation and timed search.
- Player footsteps that report walking/running noise to enemy listeners with configurable loudness and intervals.
- Optional LAN multiplayer host/client sample (Netcode for GameObjects).

## Install

Targets Unity 2022.3 LTS and newer. Unity Editor compilation and runtime testing are pending. Install via Package Manager > + > Add package from git URL:

`https://github.com/ibrahimhalil2012polat-dev/Unity-Horror-Toolkit.git`

Alternatively, choose Add package from disk and select package.json in an extracted copy. The runtime has no third-party dependencies.

## Player interaction

Add `PlayerInteractor` to the player, assign its camera and put colliders on
interactable objects. With legacy input enabled, E interacts automatically.
Projects using the new Input System can invoke `TryInteract()` from an action
callback. `PromptChanged` provides the current text for your UI; no UI package
dependency is required.

Add `DoorInteractable` to a door, `KeyPickupInteractable` beside `KeyPickup`
for manual key collection (turn off Collect On Trigger), or
`InspectableInteractable` for clues and items. See the
[interaction guide](Documentation~/Interaction.md) for setup and limitations.

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

## Flashlight and battery

Add FlashlightController to the player root and assign a child Spot Light. Configure battery life, starting charge and low-battery threshold. Connect Toggle() to your input action or UI button. Charge depletes only while on; empty batteries switch the light off. BatteryPickup restores charge on a trigger collision and is consumed only if charging succeeds. BatteryChanged exposes a normalized float for UI. See [flashlight setup](Documentation~/Flashlight.md) for detailed setup, audio events and limitations.

## Jumpscares and ambient audio

Add JumpscareController to an active manager, assign a separate visual and optional dedicated AudioSource/clip, and configure duration and volume. Use JumpscareTrigger for one-shot or repeatable player-triggered scares. RandomAmbiencePlayer schedules ambient clips with random delays and avoids immediate repeats when alternatives exist. See [scare and audio setup](Documentation~/ScaresAndAudio.md) for configuration and limitations. No visual or audio assets are bundled.

## Enemy AI

Bake a NavMesh and add EnemyBrain to an enemy with EnemyVision and NavMeshAgent. Assign the player, patrol waypoints, and obstacle layers. The brain patrols, chases visible targets, investigates reported game sounds and searches the last known position. Configure speeds, hearing radius, field of view and timers. See [enemy AI setup](Documentation~/EnemyAI.md) for setup, events, and limitations. No attack logic, models or animations are included.

## Footstep noise

Attach `FootstepNoiseEmitter` to your player and assign the `EnemyBrain`
listeners. Walking and running use different noise strengths, so running
can alert an enemy at a greater distance. The component supports automatic
movement-based steps or explicit animation events, plus optional sound events.
See the [footstep noise guide](Documentation~/FootstepNoise.md).

## LAN multiplayer

Install Netcode for GameObjects and Unity Transport first (NGO 1.8+ API target), then import LAN Multiplayer from the package Samples tab. See [setup](Samples~/LanMultiplayer/README.md). This is a direct-IP LAN example, not a production online service. No room codes, relay, matchmaking, authentication, player cap or gameplay synchronization are included.

## Testing and contributions

The package includes thirty-five Play Mode test cases covering doors, inventory, outages, flashlights, scares, audio and NPC sensing/decision states. See [testing instructions](Documentation~/Testing.md) to enable and run them. Tests have been added but not executed in Unity yet. Reproducible bug reports and actual test results are welcome.

Version 0.6.0 is an early development version, not a production-ready release.

## License

[MIT](LICENSE).
