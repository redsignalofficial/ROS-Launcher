namespace ROS.Launcher.Models;

public class GuestProfile
{
    public string Nickname { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
    public int LocalLevel { get; set; } = 1;
}
