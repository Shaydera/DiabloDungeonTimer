using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using DiabloDungeonTimer.Core.Services;
using DiabloDungeonTimer.Core.Services.Interfaces;
using DiabloDungeonTimer.Core.ViewModels;
using DiabloDungeonTimer.UI.Services;
using DiabloDungeonTimer.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DiabloDungeonTimer.UI;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private MainWindow? MdiParentWindow { get; set; }

    private static async Task SetupDependencies()
    {
        var settingsService = await XmlSettingsService.BuildSettingsServiceAsync();
        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<ISettingsService>(settingsService)
                .AddSingleton<IFileService, WindowsFileService>()
                .AddSingleton<ILogMonitorService, LogMonitorService>()
                .BuildServiceProvider());
        await Task.CompletedTask;
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        await SetupDependencies();
        MdiParentWindow = new MainWindow();
        MainWindow = MdiParentWindow;
        var viewModel = new MainWindowViewModel("Diablo IV Dungeon Timer");
        viewModel.RequestClose += MainWindowOnRequestClose;
        MdiParentWindow.DataContext = viewModel;
        MdiParentWindow.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
    }

    private void MainWindowOnRequestClose(object? sender, EventArgs e)
    {
        MdiParentWindow?.Close();
    }
}