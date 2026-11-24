# Keys, locks and feedback

## Setup

1. Add KeyInventory to the player root.
2. Add DoorController to the door hinge, enable Start Locked and set Required Key Id (e.g. basement).
3. Add KeyPickup to a key object with a 3D trigger collider, and enter the same Key Id. Trigger collection requires a Rigidbody on at least one participant (kinematic is fine).
4. Call door.Interact(playerInventory) from your player interaction code. It unlocks and opens the door with the correct key; Consume Key optionally removes it.

## Optional explicit collection

Turn off Collect On Trigger on the pickup. Call pickup.TryCollect(playerInventory) from your raycast, button or input action after checking reach and line of sight. The method returns true only on a successful new key collection. The toolkit does not implement input, raycast or range checks itself. Duplicate or blank IDs are rejected without consuming the pickup.

## Sound and UI

- Door: assign an AudioSource, Unlock Clip and Blocked Clip. Sounds use PlayOneShot; set spatial blend and volume on the source as desired.
- Pickup: assign Pickup Clip and Pickup Volume. A temporary positional audio source plays the clip independently of the deactivated key.
- Door Missing Key event sends the required ID when a locked door blocks Open or Interact; bind it to your UI text or localization handler.
- Pickup Collected event sends the collected ID once; bind it to a UI notification or puzzle callback.

Audio clips and UI assets are not included. TryUnlock is a low-level boolean check and does not report blocked feedback; Interact reports it. Unlock is an explicit scripted override. The inventory is in-memory and case-sensitive; this update does not add save/load or multiplayer synchronization.

## Verification

In Play Mode, try the door without a key and with a wrong key, then collect the correct key. Verify blocked/unlock events, door motion, audio, and optional consumption. Test both automatic and explicit collection and duplicate keys. Automated test cases cover successful pickup, duplicate rejection and missing-key events; audio and physics setup require manual testing. Unity execution has not yet been performed for this release.
