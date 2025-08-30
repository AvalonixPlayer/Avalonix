using Avalonia.Controls;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

public partial class PlaylistSelectWindow : Window
{
    public PlaylistSelectWindow(ILogger logger, PlaylistSelectWindowViewModel vm)
    {
        InitializeComponent();
        logger.LogInformation("PlaylistCreateWindow opened");
    }
}