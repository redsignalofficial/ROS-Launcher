# ROS REVIVAL — Architecture & Design Specification

## Overview

The ROS REVIVAL Launcher is a standalone Windows desktop client designed with C# and Windows Presentation Foundation (WPF). It follows clean architecture and the MVVM (Model-View-ViewModel) design pattern.

---

## Architectural Principles

1. **Separation of Concerns:**
   - **Views:** Pure XAML UI layouts with zero business logic in code-behind.
   - **ViewModels:** Handle presentation state, commands, and bindable properties.
   - **Services:** Encapsulate IO, JSON configuration, profile management, and process execution.
   - **Models:** Plain data transfer objects (DTOs) representing profiles, settings, and game state.

2. **Offline-First & Local Persistence:**
   - All configurations (`settings.json`) and guest profile records (`profile.json`) are stored locally in `%LOCALAPPDATA%\ROSRevival\`.
   - No external network requests are required to launch or operate the interface.

3. **Game Detection & Process Safety:**
   - The launcher locates `ros.exe` either automatically by scanning parent and adjacent paths, or through user-specified directory settings.
   - File integrity is validated via SHA-256 manifests.
   - The launcher monitors process startup and exit codes without injecting code or modifying game files.

---

## Layer Breakdown

### 1. Models
- `GuestProfile`: Represents the local guest player (nickname, unique local ID, created timestamp).
- `LauncherSettings`: Stores launcher preferences (game path, launch arguments, resolution, auto-close preference).
- `GameInstallation`: Holds detection state (is found, executable path, file count, validation status).

### 2. Services
- `IConfigurationService`: Handles loading and saving `LauncherSettings`.
- `IProfileService`: Manages local guest profile creation, editing, and persistence.
- `IGameDetectionService`: Discovers game folders and locates `ros.exe`.
- `IGameLaunchService`: Manages `ProcessStartInfo`, process execution, and error handling.

### 3. ViewModels
- `MainViewModel`: Orchestrates top-level navigation, window controls, and global status.
- `HomeViewModel`: Manages player profile card, game detection status, and Play action.
- `LobbyViewModel`: Manages simulated custom lobby, room creation, and map/mode selection.
- `SettingsViewModel`: Manages directory selection and launcher options.
