using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonix.Models.Disk;
using Avalonix.Models.Disk.DiskLoader;
using Avalonix.Models.Disk.DiskManager;
using Avalonix.Models.Disk.DiskWriter;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Models.UserSettings;
using Avalonix.Services;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.ThemeManager;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModels;
using Avalonix.ViewModels.Main;
using Avalonix.ViewModels.PlaylistEditOrCreate;
using Avalonix.ViewModels.PlaylistSelect;
using Avalonix.ViewModels.Strategy;
using Avalonix.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix;

public class App : Application
{
    private static IServiceProvider? ServiceProvider { get; set; }

    public override void OnFrameworkInitializationCompleted()
    {
        var host = Host.CreateDefaultBuilder();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IMainWindowViewModel, MainWindowViewModel>();
            services.AddTransient<IPlaylistEditOrCreateWindowViewModel, PlaylistEditOrCreateWindowViewModel>();
            services.AddTransient<IPlaylistSelectWindowViewModel, PlaylistSelectWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<IDiskManager, DiskManager>();
            services.AddSingleton<IDiskLoader, DiskLoader>();
            services.AddSingleton<IDiskWriter, DiskWriter>();
            services.AddSingleton<IMediaPlayer, MediaPlayer>();
            services.AddSingleton<IPlaylistManager, PlaylistManager>();
            services.AddSingleton<IThemeManager, ThemeManager>();
        }).ConfigureLogging(log =>
        {
            log.ClearProviders();
            log.AddProvider(new LoggerProvider());
        });

        var hostBuilder = host.Build();
        ServiceProvider = hostBuilder.Services;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>();

        dataValidationPluginsToRemove.ToList().ForEach(plugin => BindingPlugins.DataValidators.Remove(plugin));
    }
}