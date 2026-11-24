namespace ROS.Launcher.Models;

public class GameInstallation
{
    public bool IsDetected { get; set; }
    public string GameDirectory { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string ClientVersion { get; set; } = "Unknown";
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
    public List<string> MissingFiles { get; set; } = new();
    public List<string> ValidatedFiles { get; set; } = new();

    public bool IsReadyToPlay => IsDetected && !string.IsNullOrEmpty(ExecutablePath) && MissingFiles.Count == 0;

    public string StatusDescription
    {
        get
        {
            if (!IsDetected)
                return "Original game directory not detected. Please select the folder containing 'ros.exe'.";
            if (MissingFiles.Count > 0)
                return $"Missing essential files: {string.Join(", ", MissingFiles)}";
            return $"Game verified ({ClientVersion}) — Ready to launch";
        }
    }
}
