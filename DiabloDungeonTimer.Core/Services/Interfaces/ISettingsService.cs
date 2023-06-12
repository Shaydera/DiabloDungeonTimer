using System.Diagnostics.CodeAnalysis;
using DiabloDungeonTimer.Core.Models;

namespace DiabloDungeonTimer.Core.Services.Interfaces;

/// <summary>
///     Services which provide access and storage management for application settings
/// </summary>
[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface ISettingsService
{
    public Settings Settings { get; }
    public Task<bool> SaveAsync();
    public Task ReloadAsync();
    public bool GameDirectoryValid();
    public bool IsValid();
}