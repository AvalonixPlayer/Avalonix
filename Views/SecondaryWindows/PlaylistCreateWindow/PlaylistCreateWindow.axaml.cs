using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using static Avalonix.ViewModels.PlaylistCreateWindowViewModel;

namespace Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;

public partial class PlaylistCreateWindow : Window
{
    private readonly PlaylistCreateWindowViewModel _vm;
    private ILogger _logger;
    public PlaylistCreateWindow(ILogger logger, PlaylistCreateWindowViewModel vm)
    {
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        logger.LogInformation("PlaylistCreateWindow opened");
    }

    private async void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        await _vm.OpenTrackFileDialog(this);
    }
}