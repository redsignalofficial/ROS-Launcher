using System.IO;
using System.Text.Json;
using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public class LocalProfileService : IProfileService
{
    private readonly string _profileFilePath;

    public LocalProfileService(string? customBasePath = null)
    {
        string basePath = customBasePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ROSRevival"
        );
        Directory.CreateDirectory(basePath);
        _profileFilePath = Path.Combine(basePath, "profile.json");
    }

    public async Task<GuestProfile> LoadOrCreateProfileAsync()
    {
        try
        {
            if (File.Exists(_profileFilePath))
            {
                string json = await File.ReadAllTextAsync(_profileFilePath);
                var profile = JsonSerializer.Deserialize<GuestProfile>(json);
                if (profile != null && !string.IsNullOrWhiteSpace(profile.Nickname))
                {
                    return profile;
                }
            }
        }
        catch
        {
            // Fall back to new profile on read error
        }

        var newProfile = new GuestProfile
        {
            Nickname = GenerateDefaultGuestNickname(),
            LocalId = Guid.NewGuid().ToString("N")[..12],
            CreatedAt = DateTime.UtcNow,
            LastPlayedAt = DateTime.UtcNow,
            LocalLevel = 1
        };

        await SaveProfileAsync(newProfile);
        return newProfile;
    }

    public async Task SaveProfileAsync(GuestProfile profile)
    {
        string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_profileFilePath, json);
    }

    public string GenerateDefaultGuestNickname()
    {
        int randomId = Random.Shared.Next(1000, 9999);
        return $"Survivor_{randomId}";
    }
}
