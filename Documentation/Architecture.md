# ROS REVIVAL — Architecture & Design Specification

## Overview

The ROS REVIVAL Launcher is a standalone Windows desktop client designed with C# and Windows Presentation Foundation (WPF). It follows clean architecture and the MVVM (Model-View-ViewModel) design pattern.

---

## Architectural Principles

1. **Separation of Concerns:**
   - **Views:** Pure XAML UI layouts with zero business logic in code-behind.
   - **ViewModels:** Handle presentation state, commands, and bindable properties.
   - **Services:** Encapsulate IO, JSON configuration, profile management, lobby state, and process execution.
   - **Models:** Plain data transfer objects (DTOs) representing profiles, settings, lobby state, and game state.

2. **Offline-First & Local Persistence:**
   - All configurations (`settings.json`), guest profile records (`profile.json`), and custom lobby presets (`lobby.json`) are stored locally in `%LOCALAPPDATA%\ROSRevival\`.
   - No external network requests, authentication servers, or cloud APIs are required to operate the launcher.

3. **Atomic Writes & Corrupted Data Recovery:**
   - File persistence uses an atomic write pattern (writing first to a unique `.tmp` file in the same directory, flushing, and replacing the target file).
   - In the event of malformed or corrupted JSON, services create an automatic timestamped backup (e.g. `lobby.json.corrupted.20261124_120000`) and restore a clean default state without crashing.

4. **Game Detection & Process Safety:**
   - The launcher locates `ros.exe` either automatically by scanning parent and adjacent paths, or through user-specified directory settings.
   - File integrity is validated via SHA-256 manifests.
   - The launcher monitors process startup and exit codes without injecting code or modifying game files.
   - Starting a match launches the genuine, untouched `ros.exe` with standard process parameters; no unsupported CLI flags (`--map`, `--mode`, `--bots`) are passed to the game executable.

---

## Layer Breakdown

### 1. Models
- `GuestProfile`: Represents the local guest player (nickname, unique local ID, created timestamp, local level).
- `LauncherSettings`: Stores launcher preferences (game path, launch arguments, resolution, auto-close preference).
- `GameInstallation`: Holds detection state (is found, executable path, file count, validation status).
- `LobbyRoom`: Represents local custom match configuration (RoomId, RoomName, SelectedMap, SelectedMode, Perspective, MaxPlayers, Slots, timestamps).
- `LobbySlot`: Represents an individual team slot (SlotNumber, PlayerName, IsOccupied, IsHost, IsBot, IsReady, Role, Note).
- `CapacityChangeResult`: Details the outcome of team capacity changes (Status, NewCapacity, OccupiedSlotsAtRisk, TruncatedSlotNumbers, Message).

### 2. Services
- `IConfigurationService` / `JsonConfigurationService`: Handles loading and saving `LauncherSettings`.
- `IProfileService` / `LocalProfileService`: Manages local guest profile creation, editing, and persistence.
- `IGameDetectionService` / `GameDetectionService`: Discovers game folders and validates core files (`ros.exe`, `assets.npk`, `script.npk`, `setting.ini`).
- `IGameLaunchService` / `GameLaunchService`: Manages `ProcessStartInfo`, process execution, duplicate launch prevention, and exit telemetry.
- `ILobbyService` / `LocalLobbyService`: Manages 6-digit local room IDs, atomic JSON storage, corrupted JSON recovery, team capacity scaling, and visual placeholder bots.
- `ILauncherLogService` / `LauncherLogService`: Diagnostic logging to `%LOCALAPPDATA%\ROSRevival\Logs\launcher.log`.

### 3. ViewModels
- `MainViewModel`: Orchestrates top-level navigation, guest profile, game detection, custom lobby configuration, and game execution.
- `ViewModelBase`: Implements `INotifyPropertyChanged` with property change helpers.
- `RelayCommand`: Flexible command implementation supporting parameterless and parameterized actions.

### 4. Custom Lobby Architecture
- **6-Digit Local Room IDs**: Strictly generated in the range `100000`–`999999` for offline room identification.
- **Visual Placeholder Bots**: Documented and identified as offline UI placeholders for team composition preview; no simulated AI is injected into `ros.exe`.
- **Capacity Truncation Protection**: If a user switches from Squad (4) or Fireteam (5) to a smaller mode while excess slots are occupied, a warning banner is raised and slots are preserved until explicit user confirmation.
