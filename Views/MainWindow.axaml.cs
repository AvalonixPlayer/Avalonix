using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views;

public partial class MainWindow  : Window 
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IMainWindowViewModel _vm;

    public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm)
    {
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        _logger.LogInformation("MainWindow initialized");
    }

    private void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _logger.LogInformation("Volume slider changed");
        
    }

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e) =>
        await (await _vm.PlaylistCreateWindow_Open()).ShowDialog(this);

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e) =>
        await (await _vm.PlaylistSelectWindow_Open()).ShowDialog(this);
}