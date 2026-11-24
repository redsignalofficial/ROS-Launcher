namespace ROS.Launcher.Models;

public class LauncherSettings
{
    public string GameDirectory { get; set; } = string.Empty;
    public bool AutoCloseOnLaunch { get; set; } = false;
    public string LaunchArguments { get; set; } = string.Empty;
    public string SelectedResolution { get; set; } = "1920x1080";
    public bool Fullscreen { get; set; } = true;
    public bool IsMuted { get; set; } = false;
}
