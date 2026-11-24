# ROS REVIVAL — MASTER DEVELOPMENT & ARCHITECTURE PLAN

**Original Rules of Survival Preservation Project**  
*Version:* 1.0.0  
*Target Platforms:* Windows 10 / 11 (x64)  
*Build Environment:* GitHub Actions Cloud Infrastructure  

---

## 1. Executive Summary & Core Philosophy

**ROS Revival** is a community-driven preservation project dedicated to keeping the authentic Rules of Survival PC experience accessible. 

### Core Principles
1. **Preserve Authentic Identity**: Keep the original game atmosphere, UI aesthetic, maps, audio, characters, weapons, and vehicles.
2. **Strictly No Redesign / Cartoon Mascots**: Do not invent replacement characters, cartoon mascots, or unrelated artwork.
3. **Independent Infrastructure**: Build the launcher and future services from scratch without dependencies on NetEase or private third-party servers (e.g., ROS Infinity).
4. **No Binary Tampering**: Do not modify protected game binaries (`ros.exe`), reverse engineer anti-cheat mechanisms, or bypass encryption without authorization.
5. **Transparency & Honesty**: Never claim multiplayer or offline game functionality works unless it has been verified and tested. Clearly distinguish between a launcher-side lobby interface and live in-engine multiplayer.
6. **Mobile-First GitHub Operations**: Enable full development, compilation, CI/CD testing, and packaging via GitHub and GitHub Actions so the project can be managed without requiring a local Windows PC.

---

## 2. Workspace & Environment Inspection

### Current Environment
- **Workspace Path:** `/workspaces/ai-tool`
- **Git Origin:** `https://github.com/bomeaster/ai-tool` (Branch: `main`)
- **Local SDK:** .NET SDK 10.0.403 installed in Codespace Linux environment.
- **Game Files Present:**
  - `RosInfinity-client-1.9.0.zip` (5.9 GB compressed archive)
  - `RosInfinity/` folder containing:
    - `Game/`: Original Rules of Survival NeoX engine client (`ros.exe`, `assets.npk`, `script.npk`, `setting.ini`, x64 binaries). **Must be preserved untouched.**
    - `launcher/` & `RosInfinity.exe`: Old WebView2 web-wrapper launcher enforcing online accounts on rosinfinity.com. **Target for complete replacement.**

### Immediate Repository Cleanup Requirements
- Create `.gitignore` to ensure `RosInfinity-client-1.9.0.zip` and proprietary game binaries (`RosInfinity/Game/`) are never committed or pushed to Git.
- Preserve local game files for detection and verification testing.

---

## 3. Old Launcher Replacement Specification (Requirement 13)

### Audit of the Old Launcher
The existing launcher in `RosInfinity/` consists of:
- **Wrapper:** `RosInfinity.exe` (.NET Framework 4.6.2 WinForms executable)
- **Web Host:** Microsoft Edge WebView2 runtime dependencies (`Microsoft.Web.WebView2.Core.dll`, `Microsoft.Web.WebView2.WinForms.dll`, `WebView2Loader.dll`)
- **Frontend:** `RosInfinity/launcher/ui/` (`index.html`, `launcher.css`, `launcher.js`, `tokens.css`)
- **Flaws & Anti-Patterns:**
  - Enforces mandatory online account login to private servers (`https://rosinfinity.com/account`).
  - Outdated web-shell styling with third-party branding.
  - No offline Guest Mode.
  - Heavy runtime dependencies on WebView2.

### Replacement Strategy
| Old Component | Old Behavior | ROS Revival Replacement |
| :--- | :--- | :--- |
| **Technology** | WinForms + WebView2 Web Shell | Native C# WPF (.NET 8/10 x64) |
| **Authentication** | Mandatory online login server | Local Guest Mode (Offline by default, local JSON profile) |
| **Visual Style** | Generic third-party web interface | Modern PC Battle Royale tactical aesthetic (authentic ROS palette) |
| **Lobby UI** | None (direct play button only) | Dedicated Launcher-side Lobby (room setup, map & mode selection) |
| **Game Detection** | Hardcoded relative directory | Dynamic detection + manual selection + integrity verification |
| **Dependencies** | External WebView2 runtime | Standalone WPF desktop application |

*Note: The original game files in `RosInfinity/Game/` remain completely untouched.*

---

## 4. Technical Architecture & Project Structure

### Recommended Framework: .NET 8.0-windows (LTS) / .NET 10
- **Primary Target:** `net8.0-windows` (rock-solid LTS on GitHub Actions `windows-latest` runners, zero runtime friction for Windows 10/11 users).
- **Architecture:** x64.
- **UI Framework:** Windows Presentation Foundation (WPF) with modern XAML styling and MVVM pattern.

### Project Layout
```
ROS-REVIVAL/
├── .github/
│   └── workflows/
│       └── windows-build.yml       # Automated cloud build & artifact packaging
├── Launcher/
│   └── ROS.Launcher/
│       ├── Configuration/         # App settings & constants
│       ├── Models/                # GuestProfile, GameInstallInfo, LobbyRoom
│       ├── Services/              # Config, Profile, GameDetection, GameLaunch
│       ├── ViewModels/            # Main, Home, Lobby, Settings ViewModels
│       ├── Views/                 # MainWindow, HomeView, LobbyView, SettingsView
│       ├── Resources/             # Styles, Brushes, Icons, Themes
│       ├── app.manifest           # DPI awareness & execution level
│       └── ROS.Launcher.csproj
├── Tests/
│   └── ROS.Launcher.Tests/        # Cross-platform unit tests (runs on Linux & CI)
│       ├── ConfigServiceTests.cs
│       ├── ProfileServiceTests.cs
│       ├── GameDetectionTests.cs
│       └── ROS.Launcher.Tests.csproj
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

## 5. UI/UX Design System for ROS Revival Launcher

### Visual Identity (Authentic Tactical ROS Theme)
- **Background:** Deep tactical charcoal (`#0F1115`, `#161920`, `#1E222B`).
- **Surface & Cards:** Dark slate panels (`#252A35`) with subtle 1px border strokes (`#353B4A`).
- **Primary Accent:** Tactical Amber / Gold (`#F5A623`, `#FFB800`) reflecting the original ROS interface and rank badges.
- **Status Indicators:**
  - Online/Ready: Signal Green (`#10B981`)
  - Offline/Guest: Cool Cyan/Steel (`#06B6D4`)
  - Warning/Missing Files: Amber (`#F59E0B`)
  - Error: Crimson (`#EF4444`)
- **Typography:** Clean sans-serif (Segoe UI / modern geometric font hierarchy).

### Launcher Screens
1. **Header / Window Frame:**
   - ROS Revival branding.
   - Status badge: `[OFFLINE - GUEST MODE]`.
   - Window control buttons (Minimize, Maximize, Close).
2. **Navigation Rail:**
   - 🏠 **Home** (Launch, News, Client Status, Quick Play)
   - ⚔️ **Lobby** (Custom Room setup, Mode & Map Selection, Player List)
   - ⚙️ **Settings** (Game Directory, Resolution, Audio, Profile Customization)
   - ℹ️ **About** (Project philosophy, Disclaimers, Open-source notices)
3. **Home Screen:**
   - Hero banner with original ROS atmosphere.
   - Guest Profile Card (editable nickname, local rank display, guest avatar badge).
   - Game Installation Status card (Detected path, game executable status, version info).
   - Primary **PLAY** / **LAUNCH GAME** action button with state management (Checking, Ready, Running).
4. **Lobby Screen (Launcher-Side):**
   - Simulated room setup for local or future LAN/P2P/server matchmaking.
   - Mode selector: Solo, Duo, Squad, Fireteam.
   - Map selector: Ghillie Island, Fearless Fiord.
   - Room player slots (Local Guest host, simulated slots).
5. **Settings Screen:**
   - Game executable path browser.
   - Verify integrity button (scans `Game/` directory against known file list).
   - Launch argument configuration.

---

## 6. GitHub Actions Cloud CI/CD Pipeline

Because development is managed from a mobile environment without a local PC, GitHub Actions is the core build engine.

### Workflow Specification (`.github/workflows/windows-build.yml`)
- **Triggers:**
  - `push` to `main`
  - `pull_request` to `main`
  - `workflow_dispatch` (manual trigger from GitHub mobile app / web interface)
- **Runner:** `windows-latest`
- **Steps:**
  1. **Checkout:** `actions/checkout@v4`
  2. **SDK Setup:** `actions/setup-dotnet@v4` with .NET 8.0 SDK
  3. **Dependency Restore:** `dotnet restore`
  4. **Code Quality & Validation:** Verify clean compilation
  5. **Unit Tests:** `dotnet test Tests/ROS.Launcher.Tests/ --configuration Release --logger "trx"`
  6. **Publish Launcher:**
     ```bash
     dotnet publish Launcher/ROS.Launcher/ROS.Launcher.csproj \
       --configuration Release \
       --runtime win-x64 \
       --self-contained false \
       -p:PublishSingleFile=true \
       -o ./artifacts/publish
     ```
  7. **Archive & Packaging:** Create `ROS-Revival-Launcher-v{version}-win-x64.zip`
  8. **Artifact Upload:** `actions/upload-artifact@v4` with 90-day retention
  9. **Summary Generation:** Markdown summary table in GitHub Actions step summary showing build duration, commit SHA, and release package size.

---

## 7. Dependencies & Security Boundaries

### NuGet Dependencies
- **Core:** Standard BCL (`System.Text.Json`, `System.IO`, `System.Diagnostics`).
- **MVVM:** `CommunityToolkit.Mvvm` (official Microsoft high-performance MVVM library).
- **Testing:** `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`.
- **Zero Proprietary/Black-Box Dependencies:** No WebView2, no NetEase SDKs, no external analytics or telemetry.

### Legal and Technical Boundaries
- **No Reverse-Engineering of Anti-Cheat:** We will not bypass, patch, or disable game anti-cheat drivers or protections.
- **No Redistribution of Game Assets:** The launcher build artifact will only contain our open-source launcher binaries. Original game assets must be provided by the user in their own game directory.
- **Offline / Server Reality Check:** The original `ros.exe` client expects to connect to a NetEase game server gateway upon boot. Without a live backend responding to its network packets, launching `ros.exe` will initialize the engine window and halt at server connection. The launcher will clearly document this state rather than claiming offline gameplay works out of the box.

---

## 8. Feasibility Analysis: Mobile vs. Cloud vs. Windows

| Activity | Mobile / Web GitHub | Cloud (Linux / Codespace) | Cloud (Windows Actions Runner) | Local Windows PC |
| :--- | :---: | :---: | :---: | :---: |
| Editing C# & XAML Code | ✅ | ✅ | — | ✅ |
| Triggering Builds | ✅ | ✅ | ✅ | ✅ |
| Running Cross-Platform Tests | — | ✅ | ✅ | ✅ |
| Compiling WPF Desktop App | — | ❌ *(Needs Windows SDK)* | ✅ | ✅ |
| Packaging Release ZIP | — | — | ✅ | ✅ |
| Downloading Artifact | ✅ | ✅ | — | ✅ |
| Visual UI Verification | ❌ | ❌ | ❌ | ✅ *(Manual test)* |
| Launching `ros.exe` Direct | ❌ | ❌ | ❌ | ✅ *(Manual test)* |

**Crucial Insight:** All code writing, architecture, automated testing of logic, compilation, and ZIP packaging can be executed 100% in the cloud without a Windows PC. Visual runtime verification and actual game client process testing can be delegated to Windows community testers using the published GitHub Actions artifacts.

---

## 9. Mobile-Friendly GitHub Workflow Guide

1. **Viewing & Editing Code on Mobile:**
   - Navigate to `https://github.com/bomeaster/ai-tool` in any mobile browser or the GitHub Mobile App.
   - Press `.` (dot) on any repository page to open the web-based VS Code editor directly on your phone.
2. **Triggering Builds:**
   - Tap **Actions** tab on GitHub repository.
   - Select **Windows Launcher Build**.
   - Tap **Run workflow** -> Select `main` branch -> Tap green **Run workflow** button.
3. **Checking Status & Logs:**
   - Tap the running workflow to view live step-by-step progress.
   - Any compile errors or failed tests will be clearly highlighted in red.
4. **Downloading Launcher Artifact:**
   - When the workflow completes (green checkmark), scroll down to the **Artifacts** section.
   - Tap **ROS-Revival-Launcher-win-x64** to download the ZIP package directly to your phone or send the link to a Windows tester.

---

## 10. Implementation Roadmap (Phases)

- **Phase 1 (Immediate Next Step upon Approval):**
  - Set up `.gitignore` and repository structure.
  - Create `ROS.Launcher.csproj` and `ROS.Launcher.Tests.csproj`.
  - Implement `.github/workflows/windows-build.yml`.
  - Validate cloud compilation on Windows runner.
- **Phase 2 (Core Launcher UI & Guest Mode):**
  - Implement modern XAML theme (Styles, Brushes, Controls).
  - Implement Guest Profile system with local JSON storage (`profile.json`, `settings.json`).
  - Implement Navigation Rail, Home View, and Settings View.
- **Phase 3 (Launcher Lobby System):**
  - Implement custom Lobby UI (Room creation, map/mode pickers, player list).
- **Phase 4 (Game Detection & Process Management):**
  - Implement directory scanner for `ros.exe` and `Game/` assets.
  - Implement file integrity checker using SHA-256 manifests.
  - Implement safe game execution runner with error handling.
- **Phase 5 (Future Backend Architecture - Deferred):**
  - Architectural documentation for independent authentication and match servers (post-approval).
