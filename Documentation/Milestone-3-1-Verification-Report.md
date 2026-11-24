# ROS REVIVAL — Milestone 3.1 Release Verification & UI Testing Report

## Executive Summary

Milestone 3.1 verifies the published standalone Windows x64 ROS Revival Launcher build (`ROS-Revival-Launcher-win-x64.zip`) and evaluates the live WPF user interface in a clean Windows environment prior to beginning Milestone 4.

**Verification Status:** ✅ **PASSED** (Artifact integrity, UI automation, state persistence, log cleanliness, and game immutability verified)  
**Evidence Status:** ✅ **CORRECTED & VERIFIED** (All 9 genuine screenshots audited, discrepancies resolved via Run #9)  
**Game Execution Status:** ⏸️ **TEST_BLOCKED** (Test 1 remains strictly blocked awaiting genuine physical Windows GPU testing)  
**Budget Consumed:** **$0.00** (Strict zero out-of-pocket policy maintained)  

---

## 1. Published Artifact Inspection

The published release package was downloaded directly from GitHub Actions Build Run #9 (commit `6fc7f28`) and inspected:

- **Artifact Name:** `ROS-Revival-Launcher-win-x64.zip`
- **ZIP File Size:** 180,680 bytes (0.17 MB)
- **Archive Package Contents:**
  - `ROS.Launcher.exe` (359,620 bytes) — Standalone single-file PE32+ executable (GUI) x86-64.
  - `ROS.Launcher.pdb` (38,188 bytes) — Diagnostic symbol file.
- **Verification:**
  - Zero missing dependencies or unmanaged DLLs.
  - Self-extracting single-file packaging executes cleanly on a standard Windows environment.

---

## 2. Evidence Audit & Discrepancy Resolution (Run #8 vs Run #9)

During review of the initial Run #8 evidence, specific UI binding, z-order, and layout discrepancies were identified. These have been completely resolved and re-verified in GitHub Actions Run #9 (Run ID: `45405068096`):

| Evidence Item / Review Requirement | Run #8 Status (Initial) | Run #9 Status (Corrected) | Technical Root Cause & Resolution |
| :--- | :--- | :--- | :--- |
| **1. Home Initial View (`01_home_initial_view.png`)** | ⚠️ Foreground blocked by Windows Terminal / GitHub Actions logs | ✅ **Verified Genuine Home View** | `Process.Start` left terminal in foreground. Implemented Win32 API window control (`SetForegroundWindow`, `BringWindowToTop`, `ShowWindow(SW_RESTORE)`, `SetWindowPos(HWND_TOPMOST)`), `Topmost="True"` in XAML, and an active UI Automation element discovery loop. Screenshot 01 now shows the clean ROS Launcher Home screen. |
| **2. Visual Bots Added (`04_lobby_visual_bots_added.png`)** | ⚠️ Roster rendered open slots instead of visual placeholder bots | ✅ **Verified Populated Roster** | `LobbyRoom.Slots` was a standard `List<LobbySlot>` lacking `INotifyCollectionChanged`, and `LobbySlot` lacked `INotifyPropertyChanged`. Converted to `ObservableCollection<LobbySlot>` and implemented `INotifyPropertyChanged`. Slots #2 and #3 now visibly display `Training Bot #02` and `Training Bot #03` with badges. |
| **3. Capacity Reduction Warning (`05_lobby_capacity_warning.png`)** | ⚠️ Warning text referenced occupied slot #3, but slot #3 visually appeared open | ✅ **Verified Occupied Slot #3 under Warning Banner** | With reactive collection and property notifications active, Slot #3 is visibly occupied by `Training Bot #03` when reducing Squad (4) to Duo (2), aligning the warning banner text with the visible roster. |
| **4. Capacity Truncation Confirm (`06_lobby_capacity_truncated.png`)** | ⚠️ Questioned whether result genuinely flowed from warning | ✅ **Verified Genuine Result** | User confirmation invokes `ConfirmChangeModeCommand`, truncating excess occupied slot #3 and open slot #4, preserving Host in slot 1 and bot in slot 2. Warning banner dismissed; Duo mode (2/2) rendered. |
| **5. Layout Clipping at 1024x768** | ⚠️ 1080x700 window clipped 56px horizontally on 1024x768 display | ✅ **Zero Clipping at 1024x768** | Sized window to `Width="980" Height="660"` centered on screen (`WindowStartupLocation="CenterScreen"`). Provides 22px horizontal margin and 54px vertical margin with zero clipping. |
| **6. Clear Buttons on Empty Slots** | ⚠️ Clear button was visible across all slots including open and host slots | ✅ **Hidden on Open & Host Slots** | Added `CanClear => IsOccupied && !IsHost` on `LobbySlot` and bound `Button.Visibility` using `BooleanToVisibilityConverter`. Clear button is now hidden on Slot 1 (Host) and all Open slots; visible only on occupied bot slots. |
| **7. Start Match Disabled Feedback** | ⚠️ Button was disabled with no explanatory message or tooltip | ✅ **Tooltip & Visible Disclaimer Active** | Added `ToolTipService.ShowOnDisabled="True"` on `BtnStartMatch` with explanatory tooltip and a visible amber disclaimer: *"ROS Installation Required"*. |
| **8. Original ROS Files Immutability** | ✅ Verified untouched | ✅ **Verified Untouched** | Baseline SHA-256 hashes for `ros.exe`, `assets.npk`, and `script.npk` confirmed 100% byte-for-byte identical. |
| **9. Non-Fabricated Evidence** | ✅ Genuine automated capture | ✅ **Genuine Automated Capture** | All 9 screenshots captured directly by `run-windows-tests.ps1` in GitHub Actions Windows runner. Zero synthetic or edited imagery. |

---

## 3. Windows UI Runtime Evidence Matrix (Run #9)

The published `ROS.Launcher.exe` was executed in a clean Windows environment (Windows Server 2025 Datacenter, Build 26100.0, Win32NT x64, 1024x768 resolution). The resulting genuine screenshots:

| Step | State Verified | Evidence Screenshot | Verification Result |
| :---: | :--- | :--- | :--- |
| **1** | **Main Window Startup** | [`01_home_initial_view.png`](file:///workspaces/ai-tool/Documentation/Screenshots/01_home_initial_view.png) | ✅ PASS — Clean Home screen in foreground; guest profile loaded; detection panel active; zero terminal overlay |
| **2** | **Custom Lobby Navigation** | [`02_custom_lobby_default.png`](file:///workspaces/ai-tool/Documentation/Screenshots/02_custom_lobby_default.png) | ✅ PASS — Custom Lobby tab navigated; Room `#881902` created; host Survivor_7111 in slot 1 |
| **3** | **Map Selection** | [`03_lobby_map_selected.png`](file:///workspaces/ai-tool/Documentation/Screenshots/03_lobby_map_selected.png) | ✅ PASS — Fearless Fiord (8.0 x 8.0 km Expansion) selected; status updated |
| **4** | **Visual Bot Slots Added** | [`04_lobby_visual_bots_added.png`](file:///workspaces/ai-tool/Documentation/Screenshots/04_lobby_visual_bots_added.png) | ✅ PASS — Slot 2 (`Training Bot #02`) and Slot 3 (`Training Bot #03`) populated; Clear button visible only on bots |
| **5** | **Capacity Reduction Warning** | [`05_lobby_capacity_warning.png`](file:///workspaces/ai-tool/Documentation/Screenshots/05_lobby_capacity_warning.png) | ✅ PASS — Warning banner displayed; Slot #3 visibly occupied by Training Bot #03; truncation confirmed pending |
| **6** | **Capacity Truncation Confirm** | [`06_lobby_capacity_truncated.png`](file:///workspaces/ai-tool/Documentation/Screenshots/06_lobby_capacity_truncated.png) | ✅ PASS — Duo mode (2P) confirmed; occupied slot #3 removed; slot 2 preserved; warning banner dismissed |
| **7** | **6-Digit Local Room Creation** | [`07_lobby_new_room_created.png`](file:///workspaces/ai-tool/Documentation/Screenshots/07_lobby_new_room_created.png) | ✅ PASS — Created new local room `#623470` (strictly offline 6-digit identifier); roster reset cleanly |
| **8** | **Perspective Selection** | [`08_lobby_perspective_fpp.png`](file:///workspaces/ai-tool/Documentation/Screenshots/08_lobby_perspective_fpp.png) | ✅ PASS — Switched perspective from TPP to FPP (First-Person); parameters updated |
| **9** | **Preset Persistence** | [`09_lobby_preset_saved.png`](file:///workspaces/ai-tool/Documentation/Screenshots/09_lobby_preset_saved.png) | ✅ PASS — Configuration written to `%LOCALAPPDATA%\ROSRevival\lobby.json`; confirmation status displayed |

---

## 4. Log & Error Audit

Application logs written to `%LOCALAPPDATA%\ROSRevival\Logs\launcher.log` during the Run #9 live session:

- **Total Logged Events:** 19
- **Error Count (`[ERROR]`):** **0**
- **Warning Count (`[WARN]`):** 1 (Expected capacity reduction warning: *"Reducing capacity to Duo (2 players) would remove occupied slot(s): #3. Confirm to proceed."*)
- **Unhandled Exceptions:** **0**
- **Atomic File Writes:** Verified 7 successful atomic file replacements to `lobby.json`.

```
[2026-11-24 13:37:01.898] [INFO] ROS Revival Launcher initialized.
[2026-11-24 13:37:03.986] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #881902
[2026-11-24 13:37:03.994] [INFO] Game detection completed: Not found in candidate paths.
[2026-11-24 13:37:03.995] [INFO] Game status refreshed. Path: '', Ready: False
[2026-11-24 13:37:08.666] [INFO] Custom Lobby map changed to 'Fearless Fiord'.
[2026-11-24 13:37:08.668] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #881902
[2026-11-24 13:37:10.595] [INFO] Added visual placeholder bot to Slot 2. Notice: Offline UI placeholder only (no in-game AI).
[2026-11-24 13:37:10.601] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #881902
[2026-11-24 13:37:12.195] [INFO] Added visual placeholder bot to Slot 3. Notice: Offline UI placeholder only (no in-game AI).
[2026-11-24 13:37:12.200] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #881902
[2026-11-24 13:37:14.317] [WARN] Capacity reduction warning: Reducing capacity to Duo (2 players) would remove occupied slot(s): #3. Confirm to proceed.
[2026-11-24 13:37:16.427] [INFO] Confirmed capacity reduction to Duo. Slots truncated.
[2026-11-24 13:37:16.442] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #881902
[2026-11-24 13:37:18.583] [INFO] New local custom room #623470 created by Survivor_7111.
[2026-11-24 13:37:18.592] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #623470
[2026-11-24 13:37:20.682] [INFO] Custom Lobby perspective set to 'FPP'.
[2026-11-24 13:37:20.683] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #623470
[2026-11-24 13:37:22.629] [INFO] Lobby configuration saved atomically to 'C:\Users\runneradmin\AppData\Local\ROSRevival\lobby.json'. Room #623470
[2026-11-24 13:37:22.630] [INFO] Lobby configuration saved. Room ID: #623470
```

---

## 5. Game File Immutability Audit

Original Rules of Survival PC client files in `RosInfinity/Game/` audited with SHA-256 cryptographic hashes:

| File | Expected Baseline SHA-256 Hash | Post-Test SHA-256 Hash | Integrity Status |
| :--- | :--- | :--- | :---: |
| `ros.exe` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | ✅ **UNTOUCHED** |
| `assets.npk` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | ✅ **UNTOUCHED** |
| `script.npk` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | ✅ **UNTOUCHED** |

---

## 6. Automated Test Suite Audit

The full cross-platform test suite was executed across target frameworks:
- **Total Tests:** 34
- **Passed:** 34
- **Failed:** 0
- **Skipped:** 0
- **Duration:** 106 ms

```
Passed!  - Failed: 0, Passed: 34, Skipped: 0, Total: 34 - ROS.Launcher.Tests.dll (net10.0)
```

---

## 7. Strict Operational Limitations & Next Steps

1. **Test 1 Remains `TEST_BLOCKED`**: The launcher has been verified to start cleanly on Windows, but genuine DirectX 3D execution of the original Rules of Survival game process (`ros.exe`) requires an interactive physical Windows GPU machine.
2. **No In-Game AI Claimed**: Visual bot slots in the Custom Lobby are strictly offline UI placeholders for team composition preview. The launcher does not inject AI bots into `ros.exe`.
3. **No Multiplayer Claimed**: Official NetEase servers are permanently offline. Match settings are local launcher configurations.
4. **Milestone 4 Boundary**: Implementation of Milestone 4 (Game Integration Assessment & Compatibility) is held pending user review of this corrected evidence report.
