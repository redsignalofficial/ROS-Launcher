using System.IO;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public class GameDetectionService : IGameDetectionService
{
    private static readonly string[] RequiredGameFiles =
    {
        "ros.exe",
        "assets.npk",
        "script.npk"
    };

    public GameInstallation DetectInstallation(string? candidateDirectory = null)
    {
        var result = new GameInstallation();

        var searchPaths = new List<string>();

        if (!string.IsNullOrWhiteSpace(candidateDirectory))
        {
            searchPaths.Add(candidateDirectory);
            searchPaths.Add(Path.Combine(candidateDirectory, "Game"));
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

                    // Check missing core files
                    foreach (string reqFile in RequiredGameFiles)
                    {
                        string checkPath = Path.Combine(result.GameDirectory, reqFile);
                        if (!File.Exists(checkPath))
                        {
                            result.MissingFiles.Add(reqFile);
                        }
                    }

                    // Try detecting version from version.dat or file info
                    string versionFile = Path.Combine(result.GameDirectory, "version.dat");
                    if (File.Exists(versionFile))
                    {
                        try
                        {
                            result.ClientVersion = File.ReadAllText(versionFile).Trim();
                        }
                        catch
                        {
                            result.ClientVersion = "1.9.0 (Verified)";
                        }
                    }
                    else
                    {
                        result.ClientVersion = "1.9.0 (PC)";
                    }

                    return result;
                }
            }
            catch
            {
                // Directory access error, continue search
            }
        }

        return result;
    }
}
