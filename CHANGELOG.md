# Changelog

## 0.6.0

- Add camera-raycast player interaction with configurable distance, layers, trigger handling and E key (legacy Input Manager).
- Add prompt-change events and a public TryInteract API for the new Input System.
- Integrate existing doors and keys through optional DoorInteractable and KeyPickupInteractable adapters.
- Allow manual key collection while preserving existing automatic trigger collection by default.
- Add generic IInteractable contract, inspectable clues with string events and one-shot mode.
- Add six Play Mode tests and player interaction setup documentation.
- Add FootstepNoiseEmitter with configurable walk/run intervals, loudness, grounded filtering and teleport safeguards.
- Add manual footstep methods, audio events, six Play Mode tests and a footstep setup guide.

Unity compilation, test execution, input/UI and footstep/NavMesh validation are pending.

## 0.5.0

- Add EnemyVision with configurable range, field of view and obstacle layers.
- Add EnemyBrain with NavMesh patrol, chase, noise investigation and timed search states.
- Add state-change events, configurable speeds and six sensing/decision test cases.
- Add NavMesh setup and AI integration guide.

Unity compilation, test execution and navigation validation are pending.

## 0.4.0

- Add timed jumpscare presentation, optional sound, cancellation and UnityEvents.
- Add one-shot or repeatable player-filtered jumpscare zones.
- Add randomized, non-overlapping ambient audio scheduling with immediate-repeat avoidance.
- Add six Play Mode tests and scare/audio setup documentation.

Unity compilation and execution are pending.

## 0.3.0

- Add flashlight toggle, timed battery drain, charging, low-battery status and UnityEvents.
- Add single-use battery pickups that remain available when the flashlight is full.
- Add six Play Mode tests and flashlight setup documentation.

Unity compilation and execution are pending.

## 0.2.0

- Add configurable door locking, matching keys, optional consumption, state-change events and interaction API.
- Add in-memory key inventory and trigger pickup.
- Add player-filtered, resettable power outage zones.
- Add five Play Mode test cases and testing instructions.

Unity compilation and execution are pending.

## 0.1.0

Initial door and light outage components, optional LAN sample, documentation.
