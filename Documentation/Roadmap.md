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

### Milestone 3: Custom Lobby Interface (Completed)
- [x] Launcher-side custom room creation interface with unique 6-digit local room IDs.
- [x] Map selection cards: Ghillie Island (6.4km) and Fearless Fiord (8.0km).
- [x] Team mode selection: Solo (1P), Duo (2P), Squad (4P), Fireteam (5P).
- [x] Perspective toggle: TPP (Third-Person) and FPP (First-Person).
- [x] Team roster view with host controls, slot roles, and notes.
- [x] Visual placeholder bot slots (+Add Bot, Clear Slot) clearly documented as offline UI placeholders.
- [x] Capacity reduction protection: prevents silent data loss with user warnings and confirmation.
- [x] Safe JSON persistence: atomic file writes and automatic recovery from corrupted/malformed JSON.
- [x] Clean Start Match workflow: launches genuine `ros.exe` without invalid CLI switches, explaining that official servers are offline.
- [x] Comprehensive test suite expanded to 34 passing automated unit and integration tests.
- [x] Volunteer Runner Security Guide created (`Documentation/Volunteer-Runner-Security-Guide.md`).

### Milestone 4: Game Integration Assessment & Compatibility (Pending Review)
- [ ] In-depth process diagnostics and exit code telemetry.
- [ ] Real-play execution on authorized volunteer Windows GPU test environment.
- [ ] Test 1 verification on physical DirectX 3D hardware (currently `TEST_BLOCKED`).

### Milestone 5: Future Independent Backend (Deferred)
- [ ] Independent player account API.
- [ ] Independent lobby matchmaking server.
- [ ] Dedicated game server prototype (subject to technical feasibility).
