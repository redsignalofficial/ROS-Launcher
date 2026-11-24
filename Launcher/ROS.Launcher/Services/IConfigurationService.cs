using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public interface IConfigurationService
{
    Task<LauncherSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(LauncherSettings settings);
    string GetSettingsFilePath();
}
