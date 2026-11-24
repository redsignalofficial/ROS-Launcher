# ROS REVIVAL — Milestone 3.1 Release Verification & UI Testing Report

## Executive Summary

Milestone 3.1 verifies the published standalone Windows x64 ROS Revival Launcher build (`ROS-Revival-Launcher-win-x64.zip`) and evaluates the live WPF user interface in a clean Windows environment prior to beginning Milestone 4.

**Verification Status:** ✅ **PASSED** (Artifact integrity, UI automation, state persistence, log cleanliness, and game immutability verified)  
**Game Execution Status:** ⏸️ **TEST_BLOCKED** (Test 1 remains strictly blocked awaiting genuine physical Windows GPU testing)  
**Budget Consumed:** **$0.00** (Strict zero out-of-pocket policy maintained)  

---

## 1. Published Artifact Inspection

The published release package was downloaded directly from GitHub Actions Build Run #8 and inspected:

- **Artifact Name:** `ROS-Revival-Launcher-win-x64.zip`
- **ZIP File Size:** 180,500 bytes (0.17 MB)
- **Archive Package Contents:**
  - `ROS.Launcher.exe` (359,620 bytes) — Standalone single-file PE32+ executable (GUI) x86-64.
  - `ROS.Launcher.pdb` (37,684 bytes) — Diagnostic symbol file.
- **Verification:**
  - Zero missing dependencies or unmanaged DLLs.
  - Self-extracting single-file packaging executes cleanly on a standard Windows environment.

---

## 2. Windows UI Runtime Verification

The published `ROS.Launcher.exe` was executed in a clean Windows environment (Windows Server 2025 Datacenter, Build 26100.0, Win32NT x64). Automated UI Automation drove the application through all Custom Lobby workflows and captured genuine desktop evidence:

| Step | State Verified | Evidence Screenshot | Verification Result |
| :---: | :--- | :--- | :---: |
| **1** | **Main Window Startup** | [`01_home_initial_view.png`](file:///workspaces/ai-tool/Documentation/Screenshots/01_home_initial_view.png) | ✅ PASS — UI initialized, guest profile loaded, integrity panel active |
| **2** | **Custom Lobby Navigation** | [`02_custom_lobby_default.png`](file:///workspaces/ai-tool/Documentation/Screenshots/02_custom_lobby_default.png) | ✅ PASS — Sidebar navigation to Custom Lobby view rendered cleanly |
| **3** | **Map Selection** | [`03_lobby_map_selected.png`](file:///workspaces/ai-tool/Documentation/Screenshots/03_lobby_map_selected.png) | ✅ PASS — Toggled between Ghillie Island and Fearless Fiord (8.0km) |
| **4** | **Visual Bot Slots** | [`04_lobby_visual_bots_added.png`](file:///workspaces/ai-tool/Documentation/Screenshots/04_lobby_visual_bots_added.png) | ✅ PASS — Filled slots 2 & 3 with visual placeholder bots; disclaimer shown |
| **5** | **Capacity Reduction Warning** | [`05_lobby_capacity_warning.png`](file:///workspaces/ai-tool/Documentation/Screenshots/05_lobby_capacity_warning.png) | ✅ PASS — Warning prompt displayed when dropping Squad (4) to Duo (2); slots preserved |
| **6** | **Capacity Truncation Confirm** | [`06_lobby_capacity_truncated.png`](file:///workspaces/ai-tool/Documentation/Screenshots/06_lobby_capacity_truncated.png) | ✅ PASS — User confirmation truncated excess occupied slot #3, preserved host & slot 2 |
| **7** | **6-Digit Local Room Creation** | [`07_lobby_new_room_created.png`](file:///workspaces/ai-tool/Documentation/Screenshots/07_lobby_new_room_created.png) | ✅ PASS — Created new local room `#428105` (strictly offline 6-digit identifier) |
| **8** | **Perspective Selection** | [`08_lobby_perspective_fpp.png`](file:///workspaces/ai-tool/Documentation/Screenshots/08_lobby_perspective_fpp.png) | ✅ PASS — Switched perspective from TPP to FPP |
| **9** | **Preset Persistence** | [`09_lobby_preset_saved.png`](file:///workspaces/ai-tool/Documentation/Screenshots/09_lobby_preset_saved.png) | ✅ PASS — Configuration written to `%LOCALAPPDATA%\ROSRevival\lobby.json` |

---

## 3. Log & Error Audit

Application logs written to `%LOCALAPPDATA%\ROSRevival\Logs\launcher.log` during the live session were retrieved and inspected:

- **Total Logged Events:** 19
- **Error Count (`[ERROR]`):** **0**
- **Warning Count (`[WARN]`):** 1 (Expected capacity reduction warning: *"Reducing capacity to Duo (2 players) would remove occupied slot(s): #3. Confirm to proceed."*)
- **Unhandled Exceptions:** **0**
- **Atomic File Writes:** Verified 7 successful atomic file replacements to `lobby.json`.

---

## 4. Game File Immutability Audit

Original Rules of Survival PC client files in `RosInfinity/Game/` were audited before and after test execution using SHA-256 cryptographic hashes:

| File | Expected SHA-256 Hash | Post-Test SHA-256 Hash | Integrity Status |
| :--- | :--- | :--- | :---: |
| `ros.exe` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | ✅ **UNTOUCHED** |
| `assets.npk` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | ✅ **UNTOUCHED** |
| `script.npk` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | ✅ **UNTOUCHED** |

---

## 5. Automated Test Suite Audit

The full cross-platform test suite was executed across target frameworks:
- **Total Tests:** 34
- **Passed:** 34
- **Failed:** 0
- **Skipped:** 0
- **Duration:** 144 ms

```
Passed!  - Failed: 0, Passed: 34, Skipped: 0, Total: 34 - ROS.Launcher.Tests.dll (net10.0)
```

---

## 6. Strict Operational Limitations & Next Steps

1. **Test 1 Remains `TEST_BLOCKED`**: The launcher has been verified to start cleanly on Windows, but genuine DirectX 3D execution of the original Rules of Survival game process (`ros.exe`) requires an interactive physical Windows GPU machine.
2. **No In-Game AI Claimed**: Visual bot slots in the Custom Lobby are strictly offline UI placeholders for team composition preview. The launcher does not inject AI bots into `ros.exe`.
3. **No Multiplayer Claimed**: Official NetEase servers are permanently offline. Match settings are local launcher configurations.
4. **Readiness for Milestone 4**: Milestone 3.1 verification is complete. Milestone 4 (Game Integration Assessment & Compatibility) will begin upon user review.
