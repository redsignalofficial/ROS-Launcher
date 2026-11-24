using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public interface IGameDetectionService
{
    GameInstallation DetectInstallation(string? candidateDirectory = null);
}
