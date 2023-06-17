namespace DiabloDungeonTimer.Core.Services.Interfaces;

/// <summary>
///     Services which provides file and folder operations
/// </summary>
public interface IFileService
{
    public string BrowseDirectory(string initialDirectory);
}