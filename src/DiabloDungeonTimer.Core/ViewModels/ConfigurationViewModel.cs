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
    private readonly ISettingsProvider _settingsProvider;

    private string _lastError = string.Empty;
    private bool _saveEnabled = true;

    public ConfigurationViewModel(ISettingsProvider? settingsProvider = null,
        IFileService? fileService = null)
    {
        _settingsProvider = settingsProvider ?? Ioc.Default.GetRequiredService<ISettingsProvider>();
        _fileService = fileService ?? Ioc.Default.GetRequiredService<IFileService>();
        SaveCommand = new AsyncRelayCommand(SaveConfiguration);
        BrowseGameDirectoryCommand = new RelayCommand(BrowseGameDirectory);
        CloseEnabled = _settingsProvider.IsValid();
        SaveEnabled = CloseEnabled;
    }

    public override string Error => _lastError;

    public string GameDirectory
    {
        get => _settingsProvider.Settings.GameDirectory;
        set
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value) || !Directory.Exists(value))
                return;

            SetProperty(_settingsProvider.Settings.GameDirectory, value, _settingsProvider,
                (service, directory) => service.Settings.GameDirectory = directory);
            SaveEnabled = _settingsProvider.IsValid();
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

    public bool KeepHistory
    {
        get => _settingsProvider.Settings.KeepHistory;
        set
        {
            SetProperty(_settingsProvider.Settings.KeepHistory, value, _settingsProvider,
                (service, newValue) => service.Settings.KeepHistory = newValue);
        }
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
                    if (!_settingsProvider.GameDirectoryValid())
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
        await _settingsProvider.SaveAsync();
        CloseCommand.Execute(null);
    }
}