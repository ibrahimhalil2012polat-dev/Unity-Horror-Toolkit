# Flashlight and battery setup

## Player and light

1. Create a Spot Light as a child of your first-person camera. Adjust its range, cone angle and intensity.
2. Add FlashlightController to the player root (the object containing the player's collider or its ancestor).
3. Drag the Spot Light into the Flashlight field. A Light on the same GameObject is also detected automatically.
4. Configure Initial Battery (0 to 1), Battery Life Seconds (120 by default), Low Battery Threshold (0.2 by default) and Start On.
5. Connect an existing input action or a mobile UI button to Toggle(). TurnOn(), TurnOff() and RechargeFull() are also available for UnityEvents.

The runtime does not require a particular input package. For keyboard controls in the legacy Input Manager, an optional integration script may call Toggle when Input.GetKeyDown(KeyCode.F) is true; use an Input System action if your project uses the new Input System.

Battery level is a normalized fraction. At a life of 120 seconds, a full battery lasts 120 seconds of scaled game time while the controller is on. It does not drain while off. A depleted battery switches the light off and blocks activation; charging does not automatically switch it on. Disabling the controller turns the light off. The battery is not saved across scenes or application restarts.

## Collectible battery

Create a GameObject with a 3D collider set to Is Trigger, add BatteryPickup and choose Charge Amount (0.5 restores half a full battery). Ensure at least one participant has a Rigidbody (kinematic is fine). The pickup searches the entering collider's parents for FlashlightController. It deactivates after a successful charge; a full battery leaves the pickup available. This is a single-use pickup, not a respawning system.

## UI and audio

Read BatteryLevel (0 to 1) or subscribe to BatteryChanged for a UI slider or text. For example, a Unity UI Slider with min 0 and max 1 can receive its SetValueWithoutNotify method through the dynamic float event. Initialize the UI from BatteryLevel when the UI is enabled: initialization does not emit an event.

TurnedOn, TurnedOff and BatteryDepleted can trigger sound effects. IsLowBattery reports whether the configured threshold has been reached; the package does not add automatic flicker or audio assets.

## Integration limitations

The controller owns the Light.enabled state of its assigned light. Do not assign the same Light to PowerOutage or other scripts controlling enabled state: use separate scene lights for blackouts. The package does not include network replication, save data, input bindings or a premade prefab. The included automated tests have not yet been executed in Unity.
