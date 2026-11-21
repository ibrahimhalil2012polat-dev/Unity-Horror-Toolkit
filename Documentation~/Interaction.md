# Player interaction system

Reusable first-person interaction with doors, keys and inspectable objects. No UI assets or
input-system package dependencies are included.

## Player setup

1. Add `PlayerInteractor` to the player root, with a `KeyInventory` on that object
   or one of its parents. Assign the view camera (or leave empty to use Camera.main).
2. Set Interaction Distance (default 3 metres), Interaction Layers and Trigger
   Interaction. The default raycast includes trigger colliders. Objects or trigger
   volumes closer to the camera can block the target.
3. With Unity's legacy Input Manager enabled, E works automatically by default.
   With the new Input System only, connect your interaction action's `performed`
   callback to `PlayerInteractor.TryInteract()`. The package does not depend on
   `com.unity.inputsystem`; it does not create or configure an Input Actions asset.
   Disable Enable Legacy Input if another input handler calls TryInteract, to
   avoid processing a single press twice in projects with both input backends.
4. Subscribe to `PromptChanged` (a `UnityEvent<string>`) and display the string
   on your UI; an empty string means hide the prompt. In a script you can call
   `interactor.PromptChanged.AddListener(UpdatePrompt)` and use `CurrentPrompt`
   for the initial state. Localize or customize each interactable's prompt in
   the Inspector.

## Doors

- Add `DoorController` to the door hinge as described in the package README.
- Add `DoorInteractable` to that object, or to a child with a collider. It
  finds the DoorController on a parent if the reference is not assigned.
- The prompt changes between Open door, Close door and Unlock door.
- Key checks, consume-key settings and blocked events still use the existing
  `DoorController.Interact(KeyInventory)` method.

## Manual key pickup

- Add `KeyPickup` and `KeyPickupInteractable` to a key object with a collider,
  or put the interactable on its child collider.
- **Disable Collect On Trigger** on KeyPickup if the key must only be collected
  by pressing E. Trigger collection stays enabled by default for existing users.
- KeyPickupInteractable uses the player's KeyInventory and refuses duplicate
  key IDs. The key deactivates only if collection succeeds.
- If the collider is a trigger, keep PlayerInteractor's Trigger Interaction
  set to Collide. For raycasting only, Rigidbody is not required.

## Inspectable objects

- Add a collider and `InspectableInteractable` to a letter, item or prop.
- Enter Prompt and Description, then hook its `Inspected` string event to your
  dialogue or inspection UI. Enable One Shot to allow a single inspection per
  enabled scene instance; call `ResetInspection()` to allow another.
- This component reports the description; it does not pause the game, lock
  player movement, save state, display a built-in panel or play animations.

## Adding custom interactions

Implement `HorrorToolkit.IInteractable` on a `MonoBehaviour`:

```csharp
using UnityEngine;

public sealed class SwitchInteractable : MonoBehaviour, HorrorToolkit.IInteractable
{
    public string Prompt => "Flip switch";

    public bool CanInteract(HorrorToolkit.PlayerInteractor player) => isActiveAndEnabled;

    public void Interact(HorrorToolkit.PlayerInteractor player)
    {
        Debug.Log("Switch flipped!");
    }
}
```

Put a collider on that GameObject or one of its children. The raycast finds
enabled IInteractable components on the hit collider or its parents. The first
eligible component at the nearest hit location is used.

## Limitations

- First-person camera-centre raycast, not a third-person cursor system.
- One hit per raycast; a non-interactable object or trigger in front blocks the ray.
- Prompt text is English by default, but can be changed in the Inspector.
- KeyInventory is in-memory only; there is no save/load or multiplayer sync.
- UI, models, sound and animation must be supplied by your game.
