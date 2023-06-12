using System.IO;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DiabloDungeonTimer.Core.Services.Interfaces;

namespace DiabloDungeonTimer.Core.ViewModels;

/// <summary>
///     ViewModel allowing for a view to manage application settings.
/// </summary>
public sealed class ConfigurationViewModel : WorkspaceViewModel
{
    private readonly IFileService _fileService;
    private readonly ISettingsService _settingsService;

    private string _lastError = string.Empty;
    private bool _saveEnabled = true;

    public ConfigurationViewModel(string displayName, ISettingsService? settingsService = null,
        IFileService? fileService = null) : base(displayName)
    {
        _settingsService = settingsService ?? Ioc.Default.GetRequiredService<ISettingsService>();
        _fileService = fileService ?? Ioc.Default.GetRequiredService<IFileService>();
        SaveCommand = new AsyncRelayCommand(SaveConfiguration);
        BrowseGameDirectoryCommand = new RelayCommand(BrowseGameDirectory);
        CloseEnabled = _settingsService.IsValid();
        SaveEnabled = CloseEnabled;
    }

    public override string Error => _lastError;

    public string GameDirectory
    {
        get => _settingsService.Settings.GameDirectory;
        set
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value) || !Directory.Exists(value))
                return;

            SetProperty(_settingsService.Settings.GameDirectory, value, _settingsService,
                (service, directory) => service.Settings.GameDirectory = directory);
            SaveEnabled = _settingsService.IsValid();
            CloseEnabled = SaveEnabled;
        }
    }

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand BrowseGameDirectoryCommand { get; }

    public bool SaveEnabled
    {
        get => _saveEnabled;
        set => SetProperty(ref _saveEnabled, value);
    }

    public override string this[string columnName]
    {
        get
        {
            var result = string.Empty;

            switch (columnName)
            {
                case nameof(GameDirectory):
                {
                    if (!_settingsService.GameDirectoryValid())
                        result = "Invalid directory";
                    break;
                }
            }

            SetProperty(ref _lastError, result, nameof(Error));
            return result;
        }
    }

    private void BrowseGameDirectory()
    {
        GameDirectory = _fileService.BrowseDirectory(GameDirectory);
    }

    private async Task SaveConfiguration()
    {
        if (!SaveEnabled)
            return;
        InputEnabled = false;
        CloseEnabled = false;
        SaveEnabled = false;
        await _settingsService.SaveAsync();
        CloseCommand.Execute(null);
    }
}