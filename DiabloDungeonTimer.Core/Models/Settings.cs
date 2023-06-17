namespace DiabloDungeonTimer.Core.Models;

/// <summary>
///     Application Settings Record
/// </summary>
public record Settings
{
    public string GameDirectory = string.Empty;
    public bool KeepHistory = false;
}