using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using DiabloDungeonTimer.Core.Models;
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
        var xmlSaveFileService = new XmlSaveFileService();
        var settingsProvider = new SettingsProvider(new Settings(), xmlSaveFileService);
        await settingsProvider.ReloadAsync();

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<ISaveFileService>(xmlSaveFileService)
                .AddSingleton<ISettingsProvider>(settingsProvider)
                .AddSingleton<IFileService, WindowsFileService>()
                .AddSingleton<ILogMonitorService, LogMonitorService>()
                .AddSingleton<ZoneDataProvider>()
                .AddSingleton<ZoneTimerViewModel>()
                .AddTransient<ConfigurationViewModel>()
                .BuildServiceProvider());
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Task setupTask = Task.Run(SetupDependencies);
        for (var i = 0; i < 20; i++)
        {
            if (setupTask.IsCompleted)
                break;
            Thread.Sleep(100);
        }

        if (!setupTask.IsCompleted)
            throw new Exception("Dependency setup timed out.");
        MdiParentWindow = new MainWindow();
        MainWindow = MdiParentWindow;
        var viewModel = new MainWindowViewModel();
        viewModel.RequestClose += MainWindowOnRequestClose;
        MdiParentWindow.DataContext = viewModel;
        MdiParentWindow.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        var zoneTimerViewModel = Ioc.Default.GetService<ZoneTimerViewModel>();
        if (zoneTimerViewModel != null)
            Task.Run(() => zoneTimerViewModel.SaveHistoryAsync()).Wait();
    }

    private void MainWindowOnRequestClose(object? sender, EventArgs e)
    {
        MdiParentWindow?.Close();
    }
}