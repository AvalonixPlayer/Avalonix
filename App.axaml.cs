using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonix.Services;
using Avalonix.ViewModels;
using Avalonix.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix;

public class App : Application
{
    private static IServiceProvider? ServiceProvider { get; set; }
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var host = Host.CreateDefaultBuilder();
        host.ConfigureServices(services =>
        {
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<IWindowManager, WindowManager>();
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
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}