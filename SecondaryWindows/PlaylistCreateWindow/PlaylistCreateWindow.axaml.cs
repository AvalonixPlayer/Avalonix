using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Avalonix.SecondaryWindows.PlaylistCreateWindow;

public partial class PlaylistCreateWindow : Window
{
    public PlaylistCreateWindow(ILogger logger)
    {
        InitializeComponent();
        logger.LogInformation("PlaylistCreateWindow opened");
    }
}