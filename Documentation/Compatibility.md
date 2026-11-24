# ROS REVIVAL — Compatibility & Game Integration Assessment

## Client Binary Profile

- **Target Executable:** `ros.exe` (Win32/x64 PE application)
- **Engine:** NetEase NeoX Engine
- **Core Archives:** `assets.npk`, `script.npk`
- **Audio Middleware:** FMOD Ex / FMOD Event (`fmodex.dll`, `fmod_event.dll`)
- **DirectX Requirements:** DirectX 9.0c / DirectX 11 (`D3DCompiler_43.dll`, `d3dx9_43.dll`)

---

## Client Networking & Handshake Reality

The original Rules of Survival PC client (`ros.exe`) was engineered as a networked client that performs the following sequence on startup:

1. **Boot:** Loads local config (`setting.ini`, `neox.xml`).
2. **Mount:** Initializes engine subsystems and mounts `.npk` archives.
3. **Gateway Discovery:** Reaches out to NetEase authentication and patch servers (`h45na.prot`, `NEACSDK`).
4. **Handshake:** Waits for server response before advancing past the loading/title screen into the main game UI.

### Project Transparency Policy:
- The ROS REVIVAL Launcher launches the client cleanly with appropriate environment and working directory settings.
- If the official servers are offline and no independent server responds to the handshake, the client will halt at connection.
- **We do not bypass anti-cheat mechanisms or decrypt protected game code without authorization.**
- Any future multiplayer or offline support will be developed strictly through authorized interfaces or independent client/server prototypes.
