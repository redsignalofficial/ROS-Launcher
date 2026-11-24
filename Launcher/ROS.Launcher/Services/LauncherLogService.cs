using System.IO;

namespace ROS.Launcher.Services;

public class LauncherLogService : ILauncherLogService
{
    private readonly string _logFilePath;
    private readonly object _lock = new();

    public LauncherLogService(string? customBasePath = null)
    {
        string basePath = customBasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ROSRevival",
            "Logs"
        );

        Directory.CreateDirectory(basePath);
        _logFilePath = Path.Combine(basePath, "launcher.log");
    }

    public string GetLogFilePath() => _logFilePath;

    public void LogInfo(string message) => WriteEntry("INFO", message);

    public void LogWarning(string message) => WriteEntry("WARN", message);

    public void LogError(string message, Exception? ex = null)
    {
        string text = ex != null ? $"{message} | Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : message;
        WriteEntry("ERROR", text);
    }

    private void WriteEntry(string level, string message)
    {
        string line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        try
        {
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
            // Fail silently if logging cannot write
        }
    }
}
