using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.LogInformation("Settings window is closed");
        base.OnClosed(e);
    }
}

