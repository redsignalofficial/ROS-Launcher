# ROS REVIVAL — Build & Cloud Compilation Instructions

## Mobile & Cloud Build (Recommended)

Because the project is designed for developers without a dedicated Windows development PC, the primary build environment is GitHub Actions.

### Triggering a Build from Mobile:
1. Open the repository on GitHub in your mobile browser or the GitHub Mobile app: `https://github.com/bomeaster/ai-tool`.
2. Tap the **Actions** tab.
3. Select the **Windows Launcher Build** workflow.
4. Tap **Run workflow** -> choose branch `main` -> tap **Run workflow**.
5. Once the run completes with a green checkmark, tap into the run.
6. Scroll down to the **Artifacts** section and tap **ROS-Revival-Launcher-win-x64** to download the compiled ZIP.

---

## Local Windows Development (For PC Users)

### Prerequisites:
- Windows 10 (version 1809+) or Windows 11.
- .NET 8.0 SDK or .NET 10.0 SDK.
- Visual Studio 2022 (with ".NET Desktop Development" workload) or VS Code with C# Dev Kit.

### Command Line Build:
```powershell
# Restore dependencies
dotnet restore

# Run unit tests
dotnet test Tests/ROS.Launcher.Tests/ROS.Launcher.Tests.csproj

# Build Release
dotnet build Launcher/ROS.Launcher/ROS.Launcher.csproj -c Release

# Publish standalone Windows x64 binary
dotnet publish Launcher/ROS.Launcher/ROS.Launcher.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish
```

---

## Linux / Codespace Testing

On Linux or within a GitHub Codespace, you can run the cross-platform unit test suite directly:

```bash
dotnet test Tests/ROS.Launcher.Tests/ROS.Launcher.Tests.csproj
```
*(Note: WPF GUI rendering is Windows-only and will be compiled via the GitHub Actions Windows runner).*
