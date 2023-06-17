using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using DiabloDungeonTimer.Core.Models;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.Services;

public class SettingsService : ISettingsService
{
    private static readonly string FileName = AppDomain.CurrentDomain.BaseDirectory + @"\settings.xml";
    private readonly ISaveFileService _saveFileService;

    public SettingsService(Settings settings, ISaveFileService? saveFileService = null)
    {
        Settings = settings;
        _saveFileService = saveFileService ?? Ioc.Default.GetRequiredService<ISaveFileService>();
    }

    public async Task<bool> SaveAsync()
    {
        return await _saveFileService.SaveAsync(Settings, FileName);
    }
    
    public async Task ReloadAsync()
    {
        (bool, Settings?) loadResult = await _saveFileService.TryLoadAsync<Settings>(FileName);
        if (!loadResult.Item1)
        {
            Debug.WriteLine("Failed to load settings file.");
            return;
        }
        if (loadResult.Item2 != null)
            Settings = loadResult.Item2;
        if (!GameDirectoryValid()) 
            Settings.GameDirectory = string.Empty;
    }

    public Settings Settings { get; private set; }

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