# ROS REVIVAL — TEST 1: ORIGINAL GAME LAUNCH REPORT

**Test Milestone:** TEST 1 — RUN THE ORIGINAL RULES OF SURVIVAL GAME  
**Target Executable:** `ros.exe` (NetEase NeoX Engine PC Client)  
**Required Assets:** `assets.npk`, `script.npk`, `setting.ini`  
**Current Test Status:** **`TEST_BLOCKED`**  

---

## 1. Test Status Classification

| Stage | Status | Evaluation |
| :--- | :---: | :--- |
| **ENVIRONMENT_READY** | ⚠️ PENDING RUNNER | Requires Windows 10/11 x64 with active Interactive Desktop session and DirectX 3D GPU. |
| **GAME_FILES_FOUND** | ⚠️ PENDING CONFIG | Proprietary ROS game files must be configured on runner via `ROS_GAME_DIR`. |
| **GAME_PROCESS_STARTED** | ⏸️ NOT TESTED | Pending execution on authorized test machine. |
| **GAME_WINDOW_VISIBLE** | ⏸️ NOT TESTED | Pending execution on authorized test machine. |
| **LOADING_SCREEN_REACHED** | ⏸️ NOT TESTED | Pending execution on authorized test machine. |
| **MAIN_MENU_REACHED** | ⛔ BLOCKED | Client requires official NetEase gateway handshake response. |
| **CONNECTION_BLOCKED** | ⚠️ EXPECTED | Official NetEase servers are permanently offline; client will halt at network connection. |
| **GAME_CRASHED** | ⏸️ NOT OBSERVED | No unhandled native crash recorded in local audit. |
| **FINAL RESULT** | **`TEST_BLOCKED`** | **Awaiting self-hosted Windows runner with dedicated GPU and authorized game files.** |

---

## 2. Windows Test Environment Requirements & Capabilities

### Runner Audit Findings
- **Repository:** `redsignalofficial/ROS-Launcher`
- **Self-Hosted Runner Count:** `0` runners currently registered.
- **GitHub-Hosted `windows-latest` Limitation:**
  - Standard GitHub-hosted Windows runners operate in a headless service container (Session 0).
  - No physical 3D DirectX GPU is attached (Microsoft Basic Display Driver only).
  - Proprietary 6GB ROS game archives cannot be committed to Git.
  - Running 3D DirectX games on `windows-latest` causes Direct3D initialization failures (`D3DERR_NOTAVAILABLE`).

### Required Test Machine Profile:
To unblock execution of `TEST 1`, an authorized Windows machine must be registered as a GitHub Actions runner with the following specifications:

1. **Operating System:** Windows 10 (64-bit) or Windows 11 (64-bit).
2. **Session Mode:** Active, logged-in interactive user desktop session (not a Windows background service).
3. **Graphics Hardware:** Physical GPU (NVIDIA GeForce, AMD Radeon, or Intel Iris/Arc) with DirectX 9.0c / 11 support.
4. **Display:** Minimum 1280x720 desktop resolution active.
5. **Game Installation:** Authorized Rules of Survival client files in a local directory (e.g., `C:\Games\RulesOfSurvival\Game`).
6. **Runner Labels:**
   ```
   self-hosted, Windows, ROS-Test, Interactive, GPU
   ```
7. **Environment Variable:** Set runner environment variable `ROS_GAME_DIR` to the game folder path.

---

## 3. Original Game File Integrity & Baseline Hashes

Baseline SHA-256 checksums from the authorized client package:

| Original File | Baseline SHA-256 Checksum | Post-Execution Status |
| :--- | :--- | :---: |
| `ros.exe` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | **Immutability Monitored** |
| `assets.npk` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | **Immutability Monitored** |
| `script.npk` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | **Immutability Monitored** |

> [!NOTE]
> Every run of `run-ros-real-launch.ps1` hashes these files before and after process execution. If any hash changes, the runner flags a critical modification error.

---

## 4. Expected Real Game Execution Behavior

When `ros.exe` is launched on an interactive GPU machine:
1. **Engine Mount:** The NeoX engine mounts `assets.npk` and `script.npk`.
2. **Window Creation:** The game window opens, initializes DirectX, and plays the startup sound.
3. **Gateway Handshake:** The client sends an initialization handshake to NetEase authentication endpoints (`h45na.prot`, `NEACSDK`).
4. **Handshake Result (`CONNECTION_BLOCKED`):** Because NetEase game servers are permanently offline, the client will display a connection error dialog (e.g., *"Failed to connect to server"* or *"Network connection timeout"*).
5. **Preservation Principle:** We will accurately capture and record this error dialog rather than attempting to bypass anti-cheat or reverse engineer protected network packets without authorization.

---

## 5. Artifact Manifest for Test 1

| Artifact Name | Expected Contents |
| :--- | :--- |
| **`ROS-Real-Launch-Screenshots`** | `01_windows_test_environment.png`, `02_ros_process_starting.png`, `03_actual_ros_game_window.png`, `04_connection_and_screen_state.png` |
| **`ROS-Real-Launch-Videos`** | `01_ros_real_launch_session.mp4` (FFmpeg GDI desktop grab) |
| **`ROS-Real-Launch-Logs`** | `environment_diagnostics.txt`, process exit logs, hash verifications |
| **`ROS-Real-Launch-Report`** | `Real-Launch-Report.json`, `ROS-Real-Launch-Test-Report.md` |
