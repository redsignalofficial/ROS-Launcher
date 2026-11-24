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
6. **Cloud CI/CD**: Cloud-compiled Windows binaries via GitHub Actions for full management on mobile or desktop without requiring a local development PC.

---

## Repository Structure

```
ROS-REVIVAL/
├── .github/
│   └── workflows/
│       └── windows-build.yml       # Automated cloud build & ZIP artifact pipeline
├── Launcher/
│   └── ROS.Launcher/              # Native C# WPF Desktop Launcher
│       ├── Configuration/
│       ├── Models/
│       ├── Services/
│       ├── ViewModels/
│       ├── Views/
│       └── Resources/
├── Tests/
│   └── ROS.Launcher.Tests/        # Cross-platform unit tests
├── Documentation/
│   ├── Architecture.md
│   ├── Roadmap.md
│   ├── Build-Instructions.md
│   └── Compatibility.md
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
