# Jumpscares and dynamic ambience

## Jumpscare

1. Create an active manager object with JumpscareController. Assign a separate visual GameObject (a child is fine), such as an apparition or a UI image. It starts hidden.
2. Optionally assign a dedicated AudioSource with Play On Awake disabled and an AudioClip you have permission to use. Set volume and duration. The default volume is 0.5 and duration is one second.
3. Call PlayScare() from a script or UnityEvent, or configure a JumpscareTrigger on a separate 3D trigger collider. Assign the controller. Set the player root tag to Player (or choose another tag); a collider on a child is supported. At least one participant needs a Rigidbody.
4. The visual is shown and sound plays for the configured duration; StopScare() ends it early. Started and Finished UnityEvents support additional effects. Disabling the controller cleans up without firing Finished.

A trigger defaults to one-shot. ResetTrigger() rearms it. Turn off One Shot to allow reactivation on later entries, provided the scare is not already playing. Clear Required Tag to accept any collider.

Use Unscaled Time to let the timer continue during a game pause. Keep the audio source on an active object, separate from any visual that gets hidden. Do not assign the controller itself or its ancestor as the visual. This component does not change the player's camera or controls. No frightening images or sounds are bundled. Avoid rapid flashing and overly loud sounds; provide a game-level option to reduce or disable intense scare effects.

## Dynamic ambient sound

1. Create an active object with a dedicated AudioSource and RandomAmbiencePlayer. Disable Play On Awake and Loop in the source.
2. Assign the source (a source on the same object is detected automatically) and add your ambient clips, such as wind, floor creaks or distant footsteps.
3. Set Minimum Delay and Maximum Delay, and Volume (default 0.35). Delay values are clamped to at least 0.1 seconds; reversed bounds are handled.
4. Enable Play Automatically to schedule the first clip after a random delay. Call StartAmbience(), StopAmbience() or PlayNext() from events or scripts if desired.

The scheduler never intentionally overlaps clips on its own source and avoids repeating the previous clip if a different non-null clip exists. Intervals are measured from the start of playback; if a clip is still playing when the next interval expires, the scheduler waits until it ends. Use Unscaled Time for scheduling while paused (actual audio playback also depends on AudioListener pause settings). StopAmbience() and disabling the component stop the dedicated source. Set AudioSource spatial blend and mixer routing for positional or 2D ambience.

Both components own their assigned AudioSource playback; use separate sources for separate effects. No network replication, save state, audio assets, camera shake or visual prefab is included. Tests are authored but not yet run in Unity.
