using System.IO;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

public abstract class SettingsServiceBase : ISettingsService
{
    protected SettingsServiceBase(Settings settings)
    {
        Settings = settings;
    }

    public abstract Task<bool> SaveAsync();
    public abstract Task ReloadAsync();

    public Settings Settings { get; protected set; }

    public bool IsValid()
    {
        return GameDirectoryValid();
    }

    public bool GameDirectoryValid()
    {
        return Directory.Exists(Settings.GameDirectory)
               && Directory.EnumerateFiles(Settings.GameDirectory,
                   "Diablo IV.exe", SearchOption.TopDirectoryOnly).Any();
    }
}