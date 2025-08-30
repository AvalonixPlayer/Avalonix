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
    private readonly MainWindowViewModel _vm;

    public MainWindow(ILogger<MainWindow> logger, MainWindowViewModel vm)
    {
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        _logger.LogInformation("MainWindow initialized");
    }

    private void InitializeComponent() =>
        AvaloniaXamlLoader.Load(this);

    public async void ExitButton_OnClick() => await _vm.ExitAsync();

    private void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e) =>
        await (await _vm.PlaylistCreateWindow_Open()).ShowDialog(this);

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e) =>
        await (await _vm.PlaylistSelectWindow_Open()).ShowDialog(this);
}