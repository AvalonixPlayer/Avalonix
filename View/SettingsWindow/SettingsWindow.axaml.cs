using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonix.Model.UserSettings;
using Avalonix.ViewModel.Settings;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SettingsWindow;

public partial class SettingsWindow : Window
{
    private readonly ISettingsWindowViewModel _vm;
    private readonly ILogger _logger;
    public SettingsWindow(ISettingsWindowViewModel vm, ILogger logger)
    {
        _vm = vm;
        _logger = logger;
        InitializeComponent();
        _logger.LogInformation("Settings Window Open");

        var settings = _vm.GetSettingsAsync().GetAwaiter().GetResult()!;
        
        if(settings.Avalonix.MusicFilesPaths.Count > 0)
            PathBox.Text = settings.Avalonix.MusicFilesPaths[0];
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.LogInformation("Settings window is closed");
        base.OnClosed(e);
    }

    private void ApplySettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var settings = _vm.GetSettingsAsync().GetAwaiter().GetResult();
        settings!.Avalonix.MusicFilesPaths = [PathBox.Text];
        _vm.SaveSettingsAsync(
            settings);
    }

    private void ExitButton_OnClick(object? sender, RoutedEventArgs e) => Close();
}

