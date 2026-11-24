# ROS REVIVAL — Project Roadmap

## Milestone Schedule

### Milestone 1: Repository & Cloud Build Infrastructure (Current)
- [x] Create project structure and clean repository layout.
- [x] Configure `.gitignore` to protect game archives and backups.
- [x] Create native C# WPF launcher project (`ROS.Launcher`).
- [x] Implement core services (Configuration, Profile, Detection).
- [x] Implement cross-platform unit tests (`ROS.Launcher.Tests`).
- [x] Deploy GitHub Actions cloud workflow (`windows-build.yml`).
- [x] Package x64 Windows release ZIP artifact.

### Milestone 2: Launcher Interface & Offline Guest Mode
- [ ] Implement tactical dark XAML theme matching authentic ROS aesthetics.
- [ ] Guest Mode nickname generator and local JSON profile editor.
- [ ] Home view with game status badge, client path, and quick play action.
- [ ] Settings view for game path configuration and display options.

### Milestone 3: Custom Lobby Interface
- [ ] Launcher-side custom room creation interface.
- [ ] Map selection: Ghillie Island and Fearless Fiord.
- [ ] Mode selection: Solo, Duo, Squad, Fireteam.
- [ ] Player count and room host controls.

### Milestone 4: Game Integration Assessment & Integrity Check
- [ ] Game directory discovery scanner.
- [ ] SHA-256 file manifest verification.
- [ ] Safe launch execution and process monitoring.
- [ ] Compatibility error diagnostics and reporting.

### Milestone 5: Future Independent Backend (Deferred)
- [ ] Independent player account API.
- [ ] Independent lobby matchmaking server.
- [ ] Dedicated game server prototype (subject to technical feasibility).
