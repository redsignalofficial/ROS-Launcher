using System.Diagnostics;
using System.IO;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public interface IGameLaunchService
{
    bool IsGameRunning(out int processId);
    bool LaunchGame(GameInstallation installation, LauncherSettings settings, out string errorMessage);
    event EventHandler<int>? GameExited;
}

public class GameLaunchService : IGameLaunchService
{
    private readonly ILauncherLogService? _logger;
    private Process? _activeGameProcess;

    public event EventHandler<int>? GameExited;

    public GameLaunchService(ILauncherLogService? logger = null)
    {
        _logger = logger;
    }

    public virtual bool IsGameRunning(out int processId)
    {
        processId = 0;

        // 1. Check internal tracked process
        if (_activeGameProcess != null && !_activeGameProcess.HasExited)
        {
            try
            {
                processId = _activeGameProcess.Id;
                return true;
            }
            catch
            {
                // Process handle may be invalid
            }
        }

        // 2. Check running processes on the system
        try
        {
            var processes = Process.GetProcessesByName("ros");
            if (processes.Length > 0 && !processes[0].HasExited)
            {
                processId = processes[0].Id;
                return true;
            }
        }
        catch
        {
            // Process query error
        }

        return false;
    }

    public bool LaunchGame(GameInstallation installation, LauncherSettings settings, out string errorMessage)
    {
        errorMessage = string.Empty;

        // 1. Check for Duplicate Launch
        if (IsGameRunning(out int runningPid))
        {
            errorMessage = $"Rules of Survival is already running (Process ID: {runningPid}). Duplicate launch prevented.";
            _logger?.LogWarning($"Duplicate launch attempt blocked: ros.exe already running with PID {runningPid}");
            return false;
        }

        // 2. Verify Game Installation Ready
        if (!installation.IsReadyToPlay)
        {
            errorMessage = "Game client is not ready. Missing files: " + string.Join(", ", installation.MissingFiles);
            _logger?.LogWarning($"Launch rejected: Client not ready. Missing: {string.Join(", ", installation.MissingFiles)}");
            return false;
        }

        // 3. Verify Executable Path Exists
        if (!File.Exists(installation.ExecutablePath))
        {
            errorMessage = $"Original executable 'ros.exe' not found at: '{installation.ExecutablePath}'.";
            _logger?.LogError($"Executable not found at path: {installation.ExecutablePath}");
            return false;
        }

        // 4. Launch Game Process
        try
        {
            _logger?.LogInfo($"Initiating launch: '{installation.ExecutablePath}' in '{installation.GameDirectory}' with arguments: '{settings.LaunchArguments}'");

            var startInfo = new ProcessStartInfo
            {
                FileName = installation.ExecutablePath,
                WorkingDirectory = installation.GameDirectory,
                Arguments = settings.LaunchArguments ?? string.Empty,
                UseShellExecute = true
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                errorMessage = "The operating system failed to start the 'ros.exe' process.";
                _logger?.LogError("Process.Start returned null.");
                return false;
            }

            _activeGameProcess = process;
            _logger?.LogInfo($"Process started successfully. PID: {process.Id}");

            // Listen for process exit
            try
            {
                process.EnableRaisingEvents = true;
                process.Exited += (sender, args) =>
                {
                    int exitCode = -1;
                    try { exitCode = process.ExitCode; } catch { }
                    _logger?.LogInfo($"Game process PID {process.Id} exited with code: {exitCode}");
                    _activeGameProcess = null;
                    GameExited?.Invoke(this, exitCode);
                };
            }
            catch
            {
                // Event subscription may fail on some platforms if handle closed
            }

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Launch failed with error: {ex.Message}";
            _logger?.LogError("Exception during Process.Start", ex);
            return false;
        }
    }
}
