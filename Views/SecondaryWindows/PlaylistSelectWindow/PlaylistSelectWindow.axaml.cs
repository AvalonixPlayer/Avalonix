using Avalonia.Controls;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

public partial class PlaylistSelectWindow : Window
{

    private readonly PlaylistSelectWindowViewModel _vm;
    private readonly ILogger _logger;
    public PlaylistSelectWindow(ILogger logger, PlaylistSelectWindowViewModel vm)
    { 
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        _logger.LogInformation("PlaylistCreateWindow opened");
    }

}