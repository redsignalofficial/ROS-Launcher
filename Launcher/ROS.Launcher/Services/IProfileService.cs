using ROS.Launcher.Models;

namespace ROS.Launcher.Services;

public interface IProfileService
{
    Task<GuestProfile> LoadOrCreateProfileAsync();
    Task SaveProfileAsync(GuestProfile profile);
    string GenerateDefaultGuestNickname();
}
