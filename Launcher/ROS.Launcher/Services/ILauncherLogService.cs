namespace ROS.Launcher.Services;

public interface ILauncherLogService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? ex = null);
    string GetLogFilePath();
}
