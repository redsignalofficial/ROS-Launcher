# ROS REVIVAL — Project Roadmap

## Milestone Schedule

### Milestone 1: Repository & Cloud Build Infrastructure (Completed)
- [x] Create project structure and clean repository layout.
- [x] Configure `.gitignore` to protect game archives and backups.
- [x] Create native C# WPF launcher project (`ROS.Launcher`).
- [x] Implement core services (Configuration, Profile, Detection).
- [x] Implement cross-platform unit tests (`ROS.Launcher.Tests`).
- [x] Deploy GitHub Actions cloud workflow (`windows-build.yml`).
- [x] Package x64 Windows release ZIP artifact.

### Milestone 2: Original Game Detection & Launch (Completed)
- [x] Automatic and manual directory detection for original Rules of Survival (`ros.exe`).
- [x] Recursive resolution of parent/subfolder installation paths (`Game/`, `RosInfinity/Game/`).
- [x] Comprehensive integrity validation of essential client files (`ros.exe`, `assets.npk`, `script.npk`, `setting.ini`).
- [x] Real `ros.exe` execution runner with correct working directory context.
- [x] Duplicate launch prevention tracking running `ros.exe` processes.
- [x] Diagnostic logging service writing to `%LOCALAPPDATA%\ROSRevival\Logs\launcher.log`.
- [x] Modern tactical Game Integrity panel on Home screen with live status, timestamp, and Browse/Refresh controls.
- [x] Dedicated Settings tab allowing manual path input and directory browsing.
- [x] Automated unit and integration test suite expanded to 18 passing tests.

### Milestone 3: Custom Lobby Interface (Pending Review)
- [ ] Launcher-side custom room creation interface.
- [ ] Map selection: Ghillie Island and Fearless Fiord.
- [ ] Mode selection: Solo, Duo, Squad, Fireteam.
- [ ] Player count and room host controls.

### Milestone 4: Game Integration Assessment & Compatibility (Pending Review)
- [ ] In-depth process diagnostics and exit code telemetry.
- [ ] Comprehensive file hash manifest verification.
- [ ] Real-play execution on authorized cloud/self-hosted Windows GPU test environment.

### Milestone 5: Future Independent Backend (Deferred)
- [ ] Independent player account API.
- [ ] Independent lobby matchmaking server.
- [ ] Dedicated game server prototype (subject to technical feasibility).
