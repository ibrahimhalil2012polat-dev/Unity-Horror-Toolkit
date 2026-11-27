# First-person interaction

1. Add RaycastInteractor to the player root and assign the player's Camera. Assign Player Root explicitly if the script is on a child. Add KeyInventory to the player root (or assign Inventory).
2. Add HorrorInteractable to an object or a parent of its 3D collider. Set Prompt and optionally One Shot.
3. For a door, assign DoorController in the Door field. For a key, assign KeyPickup in the Key Pickup field and disable Collect On Trigger on the pickup. Assign only one of these two fields.
4. For a switch, puzzle, note or other custom object, leave both fields empty and wire On Interacted to your desired UnityEvent callback.
5. Default input is E when the legacy Input Manager is enabled. For the new Input System, call Interact() from your performed callback; disable Use Legacy Input when connecting another input to avoid double activation. A mobile UI button can call Interact() too.
6. Connect On Prompt Changed (dynamic string) to your UI text setter. Empty text means no available target.

Reach is measured from the camera ray origin (near plane). Include both targets and blocking walls in Raycast Mask; it excludes Ignore Raycast by default. Player-root colliders and unrelated trigger volumes are skipped. The nearest eligible collider is selected; a solid collider in front blocks it. Do not start the ray inside a blocking collider. Trigger targets are supported. Prompts are emitted only when text changes. Locked-door and rejected-key attempts do not consume One Shot. ResetInteraction() rearms it.

TryInteract() returns whether an action succeeded (for doors, whether the target open state changed). No UI, input actions, or network synchronization are bundled. RaycastAll allocates an array on each scan; profile before using many simultaneous interactors. Tests are authored but Unity execution is pending.
