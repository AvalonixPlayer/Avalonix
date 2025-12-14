using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Services.CacheManager;
using Avalonix.Services.DiskLoader;
using Avalonix.Services.DiskManager;
using Avalonix.Services.DiskWriter;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.AlbumManager;
using Avalonix.Services.PlayableManager.PlayboxManager;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.ThemeManager;
using Avalonix.Services.VersionManager;
using Avalonix.Services.WindowManager;
using Avalonix.View;
using Avalonix.ViewModel.Main;
using Avalonix.ViewModel.PlayableSelectViewModel;
using Avalonix.ViewModel.PlaylistEditOrCreate;
using Avalonix.ViewModel.Settings;
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
            services.AddTransient<IPlayableSelectViewModel, PlayableSelectViewModel>();
            services.AddTransient<ISettingsWindowViewModel, SettingsWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddTransient<IVersionManager, VersionManager>();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<IDiskManager, DiskManager>();
            services.AddSingleton<IDiskLoader, DiskLoader>();
            services.AddSingleton<IDiskWriter, DiskWriter>();
            services.AddSingleton<IMediaPlayer, MediaPlayer>();
            services.AddSingleton<IPlayablesManager, PlayablesManager>();
            services.AddSingleton<IPlayableManager, PlaylistManager>();
            services.AddSingleton<IPlaylistManager, PlaylistManager>();
            services.AddSingleton<IPlayboxManager, PlayboxManager>();
            services.AddSingleton<IAlbumManager, AlbumManager>();
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
