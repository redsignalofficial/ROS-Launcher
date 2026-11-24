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
- [x] Milestone 3.1: Release verification & Windows UI runtime testing with 9 genuine evidence screenshots.

### Milestone 4: Backend Feasibility & Architecture Planning (Planning Complete, Pending Review)
- [x] Independent backend architecture design (`Documentation/Milestone-4-Backend-Architecture.md`).
- [x] REST & WebSocket / SignalR API specification (`Documentation/Backend-API-Specification.md`).
- [x] Technical compatibility & feasibility assessment (`Documentation/Backend-Feasibility-Assessment.md`).
- [x] Local mock backend design proposal (`Documentation/Proposed-Local-Mock-Backend-Design.md`).
- [x] Zero-cost hosting evaluation ($0.00 budget, local loopback, Oracle Cloud Free Tier).
- [x] Preservation guardrails enforced (zero binary tampering, zero anti-cheat cracking, zero leaked code).
- [x] Test 1 status preserved as strictly `TEST_BLOCKED`.

### Milestone 5: Local Mock Backend Implementation (Pending Review)
- [ ] Build `ROS.Revival.MockBackend` standalone service.
- [ ] Implement in-memory room management and SignalR lobby hub.
- [ ] Automated integration tests using `WebApplicationFactory`.
- [ ] Launcher client integration options.

### Milestone 6: Game Client Integration & Protocol Exploration (Deferred)
- [ ] Controlled network telemetry observation on authorized Windows GPU environment.
- [ ] Test 1 verification on physical DirectX 3D hardware (currently `TEST_BLOCKED`).
