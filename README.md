# ROS REVIVAL

**Original Rules of Survival PC Experience Preservation Project**

ROS REVIVAL is a community-driven initiative dedicated to preserving the authentic Rules of Survival PC game experience.

---

## Mission & Principles

1. **Preserve Authentic Game Identity**: Maintain the original visual style, sound design, maps, weapons, vehicles, and gameplay.
2. **Zero Inventions**: No replacement characters, cartoon mascots, or unrelated game artwork.
3. **Independent Technology**: A native C# WPF launcher built from scratch with zero dependencies on NetEase or private third-party infrastructure.
4. **100% Offline Guest Mode**: No mandatory online account or registration required to launch.
5. **No Binary Tampering**: We do not modify original game binaries (`ros.exe`), reverse engineer anti-cheat mechanisms, or bypass encryption.
6. **Zero-Budget Guarantee**: Strict $0.00 cloud spending policy. Testing utilizes GitHub Actions and volunteer community hardware.
7. **Cloud CI/CD**: Cloud-compiled Windows binaries via GitHub Actions for full management on mobile or desktop without requiring a local development PC.

---

## Launcher Features

- **Guest Profile**: Offline local identity with customizable nicknames and local level tracking.
- **Game Detection & Integrity**: Automatically locates `ros.exe` and validates essential game assets (`assets.npk`, `script.npk`, `setting.ini`).
- **Custom Lobby (Milestone 3)**:
  - Unique 6-digit local room codes (offline session management).
  - Map selection (Ghillie Island, Fearless Fiord).
  - Mode selection (Solo, Duo, Squad, Fireteam) with capacity reduction warning protection.
  - Team roster with visual placeholder bot slots.
  - Safe JSON persistence with atomic file writes and automatic corrupted JSON recovery.
  - Clean "Start Match" execution without unofficial command-line parameters.
- **Diagnostic Logging**: Automated activity logs saved to `%LOCALAPPDATA%\ROSRevival\Logs\launcher.log`.
- **Comprehensive Automated Tests**: 34 unit and integration tests verifying configurations, detection, launch, and lobby persistence.
- **Milestone 3.1 Verification**: Published Windows x64 build tested in live Windows environment with 9 genuine desktop evidence screenshots, zero unhandled errors, and byte-for-byte game file immutability verified.

---

## Repository Structure

```
ROS-REVIVAL/
├── .github/
│   └── workflows/
│       ├── windows-build.yml       # Cloud build & ZIP artifact pipeline
│       └── ros-real-launch-test.yml # Real-game DirectX execution test pipeline
├── Launcher/
│   └── ROS.Launcher/              # Native C# WPF Desktop Launcher
│       ├── Configuration/
│       ├── Models/
│       ├── Services/
│       ├── ViewModels/
│       ├── Views/
│       └── Resources/
├── Tests/
│   └── ROS.Launcher.Tests/        # Cross-platform unit & integration tests
├── Documentation/
│   ├── Architecture.md
│   ├── Roadmap.md
│   ├── Build-Instructions.md
│   ├── Compatibility.md
│   ├── Cloud-Windows-GPU-Testing-Plan.md
│   ├── ROS-Real-Launch-Test-Report.md
│   ├── Volunteer-Runner-Security-Guide.md
│   ├── Milestone-3-1-Verification-Report.md
│   ├── Milestone-4-Backend-Architecture.md
│   ├── Backend-API-Specification.md
│   ├── Backend-Feasibility-Assessment.md
│   ├── Proposed-Local-Mock-Backend-Design.md
│   └── Screenshots/
├── .gitignore
├── global.json
├── README.md
└── ROS-REVIVAL-PLAN.md
```

---

## Quick Start (Mobile-Friendly)

1. Go to the **Actions** tab in this GitHub repository.
2. Select **Windows Launcher Build**.
3. Click **Run workflow** on the `main` branch.
4. When the build finishes, download the `ROS-Revival-Launcher-win-x64.zip` artifact.
5. Extract the ZIP next to your `Game` folder on Windows and run `ROS.Launcher.exe`.
