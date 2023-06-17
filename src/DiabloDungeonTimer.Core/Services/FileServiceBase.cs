using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

/// <inheritdoc />
public abstract class FileServiceBase : IFileService
{
    public abstract string BrowseDirectory(string initialDirectory);
}