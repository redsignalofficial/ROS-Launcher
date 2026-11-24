namespace ROS.Launcher.Models;

public enum CapacityChangeStatus
{
    Success,
    WarningOccupiedSlotsWouldBeRemoved,
    InvalidMode
}

/// <summary>
/// Result returned when attempting to change lobby game mode / capacity.
/// Prevents accidental or silent data loss when occupied slots would be truncated.
/// </summary>
public class CapacityChangeResult
{
    public CapacityChangeStatus Status { get; set; } = CapacityChangeStatus.Success;
    public int NewCapacity { get; set; }
    public int OccupiedSlotsAtRisk { get; set; }
    public List<int> TruncatedSlotNumbers { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
