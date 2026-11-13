# Proposed manual checks (not yet performed)

- Install in a clean Unity 2022.3 or Unity 6 project and check compilation.
- Door: toggle repeatedly, test negative angle and startOpen, verify the exact closed orientation returns.
- Outage: use initially enabled and disabled lights, repeat BeginOutage, restore early, disable mid-outage, test zero duration and verify light restoration.
- LAN: import sample, install dependencies, configure NetworkManager/player prefab, connect two instances, disconnect and reconnect. Test on two devices on the same LAN.
- Record Unity version, package versions, platform and console output for issues.
