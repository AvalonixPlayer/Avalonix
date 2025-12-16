using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
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

        if (settings.Avalonix.MusicFilesPaths.Count <= 0) return;
        for (var i = 0; i < settings.Avalonix.MusicFilesPaths.Count; i++)
        {
            if (PathsPanel.Children.Count <= i)
            {
                var textBox = new TextBox
                {
                    Text = settings.Avalonix.MusicFilesPaths[i], HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };
                textBox.TextChanged += RemovePathIfEmpty!;
                PathsPanel.Children.Add(textBox);
            }

            else
                ((TextBox)PathsPanel.Children[i]).Text = settings.Avalonix.MusicFilesPaths[i];
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _logger.LogInformation("Settings window is closed");
        base.OnClosed(e);
    }

    private void ApplySettingsButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var settings = _vm.GetSettingsAsync().GetAwaiter().GetResult();

        settings!.Avalonix.MusicFilesPaths = PathsPanel.Children.Select(t => ((TextBox)t).Text!).ToList();

        _vm.SaveSettingsAsync(
            settings);
    }

    private void ExitButton_OnClick(object? sender, RoutedEventArgs e) => Close();

    private void AddPath_OnClick(object? sender, RoutedEventArgs e)
    {
        var textBox = new TextBox
        {
            Text = "Empty", HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Left
        };
        textBox.TextChanged += RemovePathIfEmpty!;
        PathsPanel.Children.Add(textBox);
    }

    private void RemovePathIfEmpty(object sender, EventArgs e)
    {
        if (sender is TextBox tb && string.IsNullOrEmpty(tb.Text))
        {
            PathsPanel.Children.Remove(tb);
        }
    }
}