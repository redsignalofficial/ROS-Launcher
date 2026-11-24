using System.IO;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public class GameDetectionService : IGameDetectionService
{
    private readonly ILauncherLogService? _logger;

    public static readonly string[] RequiredGameFiles =
    {
        "ros.exe",
        "assets.npk",
        "script.npk",
        "setting.ini"
    };

    public GameDetectionService(ILauncherLogService? logger = null)
    {
        _logger = logger;
    }

    public GameInstallation DetectInstallation(string? candidateDirectory = null)
    {
        var result = new GameInstallation
        {
            LastChecked = DateTime.UtcNow
        };

        var searchPaths = new List<string>();

        if (!string.IsNullOrWhiteSpace(candidateDirectory))
        {
            searchPaths.Add(candidateDirectory);
            searchPaths.Add(Path.Combine(candidateDirectory, "Game"));
            searchPaths.Add(Path.Combine(candidateDirectory, "RosInfinity", "Game"));
        }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        searchPaths.Add(Path.Combine(baseDir, "Game"));
        searchPaths.Add(Path.Combine(baseDir, "RosInfinity", "Game"));
        searchPaths.Add(Path.Combine(baseDir, "..", "Game"));
        searchPaths.Add(Path.Combine(baseDir, "..", "RosInfinity", "Game"));
        searchPaths.Add(baseDir);

        foreach (string path in searchPaths.Distinct())
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    continue;

                string exePath = Path.Combine(path, "ros.exe");
                if (File.Exists(exePath))
                {
                    result.IsDetected = true;
                    result.GameDirectory = Path.GetFullPath(path);
                    result.ExecutablePath = Path.GetFullPath(exePath);

                    // Validate each required game file
                    foreach (string reqFile in RequiredGameFiles)
                    {
                        string checkPath = Path.Combine(result.GameDirectory, reqFile);
                        if (File.Exists(checkPath))
                        {
                            result.ValidatedFiles.Add(reqFile);
                        }
                        else
                        {
                            result.MissingFiles.Add(reqFile);
                        }
                    }

                    // Try detecting version from version.dat
                    string versionFile = Path.Combine(result.GameDirectory, "version.dat");
                    if (File.Exists(versionFile))
                    {
                        try
                        {
                            result.ClientVersion = File.ReadAllText(versionFile).Trim();
                        }
                        catch
                        {
                            result.ClientVersion = "1.9.0";
                        }
                    }
                    else
                    {
                        result.ClientVersion = "1.9.0 (PC)";
                    }

                    _logger?.LogInfo($"Game detected at '{result.GameDirectory}' (Ready: {result.IsReadyToPlay}, Missing: {result.MissingFiles.Count})");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Error inspecting candidate path '{path}': {ex.Message}");
            }
        }

        _logger?.LogInfo($"Game detection completed: Not found in candidate paths.");
        return result;
    }
}
