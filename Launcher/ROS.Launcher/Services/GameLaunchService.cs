using System.Diagnostics;
using System.IO;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public interface IGameLaunchService
{
    bool LaunchGame(GameInstallation installation, LauncherSettings settings, out string errorMessage);
}

public class GameLaunchService : IGameLaunchService
{
    public bool LaunchGame(GameInstallation installation, LauncherSettings settings, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!installation.IsReadyToPlay)
        {
            errorMessage = "Game client is not ready. Missing files: " + string.Join(", ", installation.MissingFiles);
            return false;
        }

        if (!File.Exists(installation.ExecutablePath))
        {
            errorMessage = $"Executable not found at '{installation.ExecutablePath}'.";
            return false;
        }

        try
        {
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
                errorMessage = "Failed to start game process.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = $"Launch error: {ex.Message}";
            return false;
        }
    }
}
