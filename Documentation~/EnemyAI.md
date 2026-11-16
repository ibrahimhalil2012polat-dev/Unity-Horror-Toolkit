# Enemy AI: patrol, chase, investigate and search

## Navigation setup

1. Create a scene with walkable geometry. In Unity Package Manager install the AI Navigation package version supported by your editor, if needed to author a NavMesh. Add a NavMeshSurface, configure its collection/layers and bake the scene.
2. Create an enemy root on the baked NavMesh. Add a NavMeshAgent, EnemyVision and EnemyBrain. EnemyBrain requires the other two components automatically. Match the agent type to the baked surface.
3. Assign the player Transform to Target on EnemyBrain. Add empty scene objects as Patrol Points, on reachable walkable ground.
4. Configure patrol, chase and investigation speeds, hearing distance, investigation duration, search duration and repath interval. The agent handles rotation and pathfinding; use a separate visual child for meshes and animations.
5. In EnemyVision, assign an optional eye transform (forward axis points toward the view direction). Otherwise it uses the root position plus Eye Height and root forward. Target Height offsets the player root upward. Configure View Distance and Field Of View (total cone angle in degrees).
6. Set Obstacle Mask to environment wall/door layers only. Exclude enemy and player collider layers. The default empty mask means no obstacle blocking. Trigger colliders are ignored. A closed door needs an appropriate solid collider to block sight.

## Behaviour

- Patrol cycles through valid waypoint transforms; with no usable points it stops. Waypoints must be reachable; this prototype does not automatically skip a waypoint whose path is incomplete.
- Chase continuously updates the last seen position while the assigned target is visible.
- Losing sight starts Search at the last known position. Search tries to reach that point, waits if it arrives, and returns to Patrol after the configured duration. The duration includes travel time.
- ReportNoise(worldPosition, loudness) starts Investigate if the source is within the hearing radius scaled by loudness (0 to 1). This is a game event, not microphone input. It ignores walls, and is ignored during Chase.
- Investigation ends on arrival or timeout, then Search begins. Seeing the target overrides either state. A new valid noise can redirect investigation/search.
- StateChanged emits the new enum value when the state changes. Connect a listener to update animation, audio or other presentation. LastKnownPosition exposes the latest seen/heard point.

Example from a game interaction script:

```csharp
public HorrorToolkit.EnemyBrain enemy;

public void MakeNoise()
{
    enemy.ReportNoise(transform.position, 0.75f);
}
```

Call from a door event, footstep system, or other gameplay event. For multiple enemies, report the event to each relevant brain using your own dispatcher.

## Limitations

This is a single-target, local NPC prototype. It does not include attacks, damage, animations, models, automatic footstep detection, network authority, or persistence. Hearing is explicit distance-only gameplay logic. Movement needs a baked NavMesh and reachable destinations; dynamic doors need appropriate navigation setup. With no enabled agent on a NavMesh the brain can still update its state, but it does not move. The six added tests cover sensing and decision transitions without navigation; actual pathfinding requires an editor test scene. No Unity compilation or runtime tests have been executed yet.
