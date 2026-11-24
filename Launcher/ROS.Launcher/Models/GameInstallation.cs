namespace ROS.Launcher.Models;

public class GameInstallation
{
    public bool IsDetected { get; set; }
    public string GameDirectory { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string ClientVersion { get; set; } = "Unknown";
    public bool IsReadyToPlay => IsDetected && !string.IsNullOrEmpty(ExecutablePath) && MissingFiles.Count == 0;
    public List<string> MissingFiles { get; set; } = new();
}
