# Unity Horror Toolkit

MIT-licensed, reusable Unity horror-game components. Early development version, independently implemented; no private game assets or source code included.

## Install

Targets Unity 2022.3 LTS and newer. Unity Editor compilation and runtime testing are pending. Install via Package Manager > + > Add package from git URL:

`https://github.com/ibrahimhalil2012polat-dev/Unity-Horror-Toolkit.git`

Alternatively, extract the archive and choose Add package from disk, then select package.json. The runtime has no third-party dependencies.

## Door

Create a hinge GameObject with the mesh as a child, initially closed. Add DoorController to the hinge. Configure local axis, angle and speed. Wire an interaction or UI button to Toggle(), Open() or Close(). Does not implement collision avoidance, locking or networking.

## Power outage

Add PowerOutage to an active GameObject, assign lights, and set duration. Call BeginOutage() from your trigger or button. Wire start and restore events to sound or other effects. Original light enabled states are restored, including if the controller is disabled mid-outage. The timer uses scaled game time. Does not modify baked lighting or emissive materials. Avoid overlapping components controlling the same lights.

## LAN multiplayer

Optional sample: install Netcode for GameObjects and Unity Transport first (NGO 1.8+ compatible API target), then import LAN Multiplayer from the package Samples tab. See [setup](Samples~/LanMultiplayer/README.md). This is a direct-IP LAN example, not a production online service. No room codes, relay, matchmaking, authentication, player cap or gameplay synchronization are included.

## Testing and contributions

See [manual checklist](Documentation~/Testing.md). Test results and reproducible bug reports are welcome. Version 0.1.0 is an untested prototype, not a production-ready release.

## License

[MIT](LICENSE).
