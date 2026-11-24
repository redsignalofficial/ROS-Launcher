# ROS REVIVAL — Milestone 3.1 Release Verification & UI Testing Report

## Executive Summary

Milestone 3.1 verifies the published standalone Windows x64 ROS Revival Launcher build (`ROS-Revival-Launcher-win-x64.zip`) and evaluates the live WPF user interface in a clean Windows environment prior to beginning Milestone 4.

**Verification Status:** ✅ **PASSED** (Artifact integrity, UI automation, state persistence, log cleanliness, and game immutability verified)  
**Evidence Status:** ✅ **AUDITED, CORRECTED & VERIFIED** (All 9 genuine screenshots audited across Runs #8, #9, and #10; selection bindings and visual states verified in lockstep)  
**Game Execution Status:** ⏸️ **TEST_BLOCKED** (Test 1 remains strictly blocked awaiting genuine physical Windows GPU testing)  
**Budget Consumed:** **$0.00** (Strict zero out-of-pocket policy maintained)  

---

## 1. Published Artifact Inspection

The published release package was downloaded directly from GitHub Actions Build Run #10 (commit `5b95f31`) and inspected:

- **Artifact Name:** `ROS-Revival-Launcher-win-x64.zip`
- **ZIP File Size:** 180,820 bytes (0.17 MB)
- **Archive Package Contents:**
  - `ROS.Launcher.exe` (359,620 bytes) — Standalone single-file PE32+ executable (GUI) x86-64.
  - `ROS.Launcher.pdb` (38,620 bytes) — Diagnostic symbol file.
- **Verification:**
  - Zero missing dependencies or unmanaged DLLs.
  - Self-extracting single-file packaging executes cleanly on a standard Windows environment.

---

## 2. Selection Binding Investigation: Real Bug vs. Capture Inconsistency

### Investigation Findings
The reported inconsistency in screenshots 04 and 06 (where the lobby header indicated `Squad` or `Duo`, while the `Fireteam` button appeared highlighted) was investigated across the ViewModel, XAML view, and OS rendering layer:

1. **ViewModel & Data Layer: VERIFIED CORRECT**
   - In `MainViewModel.cs`, properties `IsModeSolo`, `IsModeDuo`, `IsModeSquad`, `IsModeFireteam`, `IsPerspectiveTpp`, `IsPerspectiveFpp`, `IsMapGhillie`, and `IsMapFearless` were correctly defined and evaluated against `Lobby.SelectedMode`, `Lobby.Perspective`, and `Lobby.SelectedMap`.
   - `NotifyLobbyChanged()` was properly raising `PropertyChanged` notifications for all selection flags.
   - Room capacity and slots collection truncated and expanded in exact accordance with the selected mode (1 for Solo, 2 for Duo, 4 for Squad, 5 for Fireteam).
   - The header summary string (`LobbyStatusSummary`) was correctly bound and accurately rendered the active mode, perspective, map, and player counts.

2. **XAML View Layer: REAL UI BINDING BUG IDENTIFIED**
   - In `MainWindow.xaml`, the Mode buttons (`BtnModeSolo`, `BtnModeDuo`, `BtnModeSquad`, `BtnModeFireteam`), Perspective buttons (`BtnPerspTpp`, `BtnPerspFpp`), and Map buttons were declared as standard `<Button>` controls with static background attributes (`Background="#252C37"` or `#1A202C`).
   - The buttons completely lacked dynamic WPF `Style` triggers or `DataTrigger`s binding their visual background, border, or foreground to the ViewModel's `IsMode*`, `IsPerspective*`, or `IsMap*` properties.
   - Consequently, the buttons had no mechanism to visually change state when the active mode or perspective was updated.

3. **Rendering Layer: WINDOWS AERO DEFAULT FOCUS INTERACTION**
   - Standard WPF buttons without custom `ControlTemplate`s use the default Windows theme (Aero2 on Windows Server 2025 / Windows 11).
   - In the Aero2 theme, whichever button retains keyboard/tab focus is rendered with a default light-cyan focus brush (`#BEE6FD`).
   - The `BtnModeFireteam` button retained focus in the tab sequence and was therefore painted with this default light-cyan focus brush, creating the misleading appearance that `Fireteam` was permanently selected, even though the ViewModel state, room capacity, and lobby header were correctly set to `Squad` or `Duo`.

### Corrective Implementation
To eliminate both the UI binding bug and the OS focus artifact:
- Defined `SelectableButtonStyle` and `MapCardButtonStyle` in `Theme.xaml` with custom `ControlTemplate`s, removing default OS focus visuals via `FocusVisualStyle="{x:Null}"`.
- Added reactive `DataTrigger`s in `MainWindow.xaml` for all 4 team modes, both perspectives, and both maps:
  - **Active Selection:** Background `#1C3847`, Border `StatusCyanBrush` (1.5px), Foreground `#FFFFFF` (Bold).
  - **Inactive Selection:** Background `#252C37`, Border `BorderDarkBrush` (1px), Foreground `TextSecondaryBrush` (Normal).
- Added 5 new automated unit tests in `LobbyServiceTests.cs` verifying mode, capacity, map, perspective, and summary string consistency in lockstep (39 total tests passing).

---

## 3. Evidence Audit & Discrepancy Resolution Matrix (Runs #8, #9, and #10)

| Evidence Item / Review Requirement | Run #8 Status (Initial) | Run #9 Status (Interim) | Run #10 Status (Verified Final) | Technical Root Cause & Final Verification |
| :--- | :--- | :--- | :--- | :--- |
| **1. Home Initial View (`01_home_initial_view.png`)** | ⚠️ Obstructed by terminal logs | ✅ Clean Home View | ✅ **Clean Home View** | Window z-order restored via Win32 API calls (`SetForegroundWindow`, `BringWindowToTop`, `ShowWindow`, `SetWindowPos(HWND_TOPMOST)`). |
| **2. Visual Bots Added (`04_lobby_visual_bots_added.png`)** | ⚠️ Displayed open slots | ✅ Populated Bots #02 & #03 | ✅ **Populated Bots & Active Squad Highlight** | `LobbyRoom.Slots` uses `ObservableCollection<LobbySlot>` with `INotifyPropertyChanged`. Squad mode button highlighted in cyan; matches header `Mode: Squad (4P)`. |
| **3. Capacity Reduction Warning (`05_lobby_capacity_warning.png`)** | ⚠️ Slot #3 appeared open | ✅ Slot #3 occupied | ✅ **Slot #3 Occupied under Warning Banner** | Warning prompt references occupied slot #3 while slot #3 visibly displays `Training Bot #03`. |
| **4. Capacity Truncation Confirm (`06_lobby_capacity_truncated.png`)** | ⚠️ Questioned result flow | ✅ Truncated to 2 slots | ✅ **Truncated to Duo & Active Duo Highlight** | Confirming truncation removes slot #3; Duo button highlighted in cyan; matches header `Mode: Duo (2P)`. |
| **5. Layout at 1024x768** | ⚠️ 1080x700 clipped | ✅ Sized 980x660 | ✅ **Zero Clipping at 1024x768** | Window sized to 980x660 centered on screen. Full 22px horizontal and 54px vertical margins on 1024x768. |
| **6. Clear Buttons on Empty Slots** | ⚠️ Visible on all slots | ✅ Hidden on Open & Host | ✅ **Hidden on Open & Host Slots** | Bound `CanClear => IsOccupied && !IsHost` via `BooleanToVisibilityConverter`. Clear button visible only on bot slots. |
| **7. Start Match Feedback** | ⚠️ Disabled with no feedback | ✅ Tooltip & Amber Text | ✅ **Tooltip & Amber Text Active** | `ToolTipService.ShowOnDisabled="True"` active; visible *"ROS Installation Required"* text rendered. |
| **8. Game-Mode Button Selection Consistency** | ⚠️ Fireteam button falsely highlighted | ⚠️ Fireteam button falsely highlighted | ✅ **Active Mode Button Matches Header & Capacity** | Mode buttons dynamically highlight based on ViewModel state (`IsMode*`). Squad highlighted in #04, Duo highlighted in #06. |
| **9. TPP / FPP Selection Consistency** | ⚠️ Static buttons | ⚠️ Static buttons | ✅ **Active Perspective Matches Header** | Perspective buttons dynamically highlight based on `IsPerspective*`. TPP highlighted in #02-#07, FPP highlighted in #08-#09. |
| **10. Original Game Files Immutability** | ✅ Untouched | ✅ Untouched | ✅ **Untouched** | SHA-256 hashes of `ros.exe`, `assets.npk`, and `script.npk` verified byte-for-byte identical. |
| **11. Genuine Evidence** | ✅ Automated | ✅ Automated | ✅ **Genuine Automated Captures** | All 9 screenshots captured directly by `run-windows-tests.ps1` in GitHub Actions Run #10 (Run ID: `45407411114`). |

---

## 4. Windows UI Runtime Evidence Matrix (Run #10)

The published `ROS.Launcher.exe` was executed in a clean Windows environment (Windows Server 2025 Datacenter, Build 26100.0, Win32NT x64, 1024x768 resolution). All 9 resulting genuine screenshots:

| Step | State Verified | Evidence Screenshot | Verification Result |
| :---: | :--- | :--- | :--- |
| **1** | **Main Window Startup** | [`01_home_initial_view.png`](file:///workspaces/ai-tool/Documentation/Screenshots/01_home_initial_view.png) | ✅ PASS — Clean Home screen in foreground; guest profile loaded; detection panel active; zero terminal overlay |
| **2** | **Custom Lobby Default** | [`02_custom_lobby_default.png`](file:///workspaces/ai-tool/Documentation/Screenshots/02_custom_lobby_default.png) | ✅ PASS — Default state: Ghillie Island card highlighted, Squad button highlighted, TPP button highlighted; header matches `Map: Ghillie Island • Mode: Squad (4P) • Perspective: TPP • Slots: 1/4` |
| **3** | **Map Selection** | [`03_lobby_map_selected.png`](file:///workspaces/ai-tool/Documentation/Screenshots/03_lobby_map_selected.png) | ✅ PASS — Fearless Fiord card highlighted; Ghillie Island dark; Squad & TPP highlighted; header matches `Map: Fearless Fiord • Mode: Squad (4P) • Perspective: TPP • Slots: 1/4` |
| **4** | **Visual Bot Slots Added** | [`04_lobby_visual_bots_added.png`](file:///workspaces/ai-tool/Documentation/Screenshots/04_lobby_visual_bots_added.png) | ✅ PASS — Squad button highlighted; Slots #2 & #3 populated; header matches `Mode: Squad (4P) • Slots: 3/4` |
| **5** | **Capacity Reduction Warning** | [`05_lobby_capacity_warning.png`](file:///workspaces/ai-tool/Documentation/Screenshots/05_lobby_capacity_warning.png) | ✅ PASS — Warning banner displayed; Squad remains highlighted pending confirmation; Slot #3 visibly occupied |
| **6** | **Capacity Truncation Confirm** | [`06_lobby_capacity_truncated.png`](file:///workspaces/ai-tool/Documentation/Screenshots/06_lobby_capacity_truncated.png) | ✅ PASS — Duo button highlighted; Squad dark; Slot #3 removed; header matches `Mode: Duo (2P) • Slots: 2/2` |
| **7** | **6-Digit Local Room Creation** | [`07_lobby_new_room_created.png`](file:///workspaces/ai-tool/Documentation/Screenshots/07_lobby_new_room_created.png) | ✅ PASS — New local room `#847364` created; default Ghillie Island, Squad, and TPP highlighted; roster reset cleanly |
| **8** | **Perspective Selection** | [`08_lobby_perspective_fpp.png`](file:///workspaces/ai-tool/Documentation/Screenshots/08_lobby_perspective_fpp.png) | ✅ PASS — FPP button highlighted; TPP dark; header matches `Perspective: FPP` |
| **9** | **Preset Persistence** | [`09_lobby_preset_saved.png`](file:///workspaces/ai-tool/Documentation/Screenshots/09_lobby_preset_saved.png) | ✅ PASS — Configuration written to `%LOCALAPPDATA%\ROSRevival\lobby.json`; FPP & Squad remain highlighted; confirmation displayed |

---

## 5. Log & Error Audit (Run #10)

Application logs written to `%LOCALAPPDATA%\ROSRevival\Logs\launcher.log` during the Run #10 live session:

- **Total Logged Events:** 19
- **Error Count (`[ERROR]`):** **0**
- **Warning Count (`[WARN]`):** 1 (Expected capacity reduction warning: *"Reducing capacity to Duo (2 players) would remove occupied slot(s): #3. Confirm to proceed."*)
- **Unhandled Exceptions:** **0**
- **Atomic File Writes:** Verified 7 successful atomic file replacements to `lobby.json`.

---

## 6. Game File Immutability Audit

Original Rules of Survival PC client files in `RosInfinity/Game/` audited with SHA-256 cryptographic hashes:

| File | Expected Baseline SHA-256 Hash | Post-Test SHA-256 Hash | Integrity Status |
| :--- | :--- | :--- | :---: |
| `ros.exe` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | `1ca743e7c5b9fbbbb8548afecf4878bc946d52e37f9b4178e6731cc69c27d1fd` | ✅ **UNTOUCHED** |
| `assets.npk` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | `4acdaa9fbce42ec187b99626504a8c7958e4fe66a9881669cc531f73bc40a908` | ✅ **UNTOUCHED** |
| `script.npk` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | `c7bef4da296f1ed0046135dedec94558d2da55241b0e3bbb2f30b1c69933c852` | ✅ **UNTOUCHED** |

---

## 7. Automated Test Suite Audit

The full cross-platform test suite was executed across target frameworks:
- **Total Tests:** 39 (34 previous + 5 new selection consistency tests)
- **Passed:** 39
- **Failed:** 0
- **Skipped:** 0
- **Duration:** 152 ms

```
Passed!  - Failed: 0, Passed: 39, Skipped: 0, Total: 39 - ROS.Launcher.Tests.dll (net10.0)
```

---

## 8. Strict Operational Limitations & Next Steps

1. **Test 1 Remains `TEST_BLOCKED`**: The launcher has been verified to start cleanly on Windows, but genuine DirectX 3D execution of the original Rules of Survival game process (`ros.exe`) requires an interactive physical Windows GPU machine.
2. **No In-Game AI Claimed**: Visual bot slots in the Custom Lobby are strictly offline UI placeholders for team composition preview. The launcher does not inject AI bots into `ros.exe`.
3. **No Multiplayer Claimed**: Official NetEase servers are permanently offline. Match settings are local launcher configurations.
4. **Milestone 4 Boundary**: Implementation of Milestone 4 (Game Integration Assessment & Compatibility) is held pending user review of this corrected evidence report.
