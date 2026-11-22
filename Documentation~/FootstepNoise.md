# Footstep noise detection

`FootstepNoiseEmitter` sends player footstep events to the existing
`EnemyBrain.ReportNoise(worldPosition, loudness)` API. It is a lightweight
single-player gameplay feature: it does **not** record microphone audio and
does **not** play any sound clips itself.

## Setup

1. Add `FootstepNoiseEmitter` to the moving player root. Drag your enemy objects
   with `EnemyBrain` components into the `Listeners` array.
2. On each EnemyBrain, tune Hearing Distance; the effective noise radius is
   `Hearing Distance * Loudness` (both steps use the same EnemyBrain logic).
   The defaults are 0.4 loudness for walking and 1.0 for running.
3. If using a CharacterController, assign it to `Character Controller` and
   keep Grounded Only enabled to suppress jumping footsteps. Without a
   CharacterController reference, automatic detection assumes grounded.
4. Set Minimum Move Speed, Running Speed, Maximum Move Speed,
   Walking Step Interval and Running Step Interval to match your player.
   Default thresholds are 0.2, 3.5 and 12 metres/second; intervals are 0.55
   and 0.35 seconds.
5. In Play Mode, walk or run near the enemy. The emitter estimates horizontal
   movement from frame-to-frame transform changes in LateUpdate, then reports
   periodic noises. The enemy investigates the location if it can hear them.

## Optional animation events and audio

For animation-synchronized footsteps, disable Automatic Detection and use
animation events or player scripts to call either
`EmitWalkingFootstep()` or `EmitRunningFootstep()`.
Avoid using both automatic and animation-event steps at the same time or
footsteps will be reported twice.

`WalkingStep` and `RunningStep` are UnityEvents you can connect to
different AudioSources in the Inspector. The events fire when a footstep is
reported, even if no enemy happens to hear it. No audio files are included.

Custom movement controllers can disable Automatic Detection and call
`TickMovement(worldPosition, deltaTime, isGrounded)` with movement samples.
Calling this while Automatic Detection is enabled may double-count motion.
Use `ResetTracking()` after scripted teleports when appropriate.

## Limitations

- Explicit enemy references; listeners are not discovered automatically.
- Gameplay hearing is based on 3D distance and ignores walls, doors and
  material absorption.
- Footstep estimation is approximate, not synchronized to animation frames.
- A sudden change faster than Maximum Move Speed is ignored to reduce
  false footsteps from teleports. Unusually fast characters need a higher
  configured threshold.
- Works with scaled game time; when the game is paused, it does not advance
  the automatic timer.
- Does not include hiding, crouching, surface materials, audio assets,
  networking, animation state detection or saves.
- EnemyBrain still ignores noises during Chase.
- Six Play Mode tests are provided but have not been run in Unity yet.
