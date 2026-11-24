using System.IO;
using System.Text.Json;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public class JsonConfigurationService : IConfigurationService
{
    private readonly string _settingsFilePath;

    public JsonConfigurationService(string? customBasePath = null)
    {
        string basePath = customBasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ROSRevival"
        );
        Directory.CreateDirectory(basePath);
        _settingsFilePath = Path.Combine(basePath, "settings.json");
    }

    public string GetSettingsFilePath() => _settingsFilePath;

    public async Task<LauncherSettings> LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<LauncherSettings>(json);
                if (settings != null) return settings;
            }
        }
        catch
        {
            // Fall back to defaults on parse/read error
        }

        var defaultSettings = new LauncherSettings();
        await SaveSettingsAsync(defaultSettings);
        return defaultSettings;
    }

    public async Task SaveSettingsAsync(LauncherSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFilePath, json);
    }
}
