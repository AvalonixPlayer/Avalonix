using Avalonia.Controls;
using Avalonix.Models.Media.Track;
using Avalonix.Services.WindowManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.ShowTrackWindow;

public partial class ShowTrackWindow : Window
{
    public ShowTrackWindow(ILogger<WindowManager> logger, Track track)
    {
        InitializeComponent();   
        logger.LogInformation("ShowTrackWindow initialized");
        Title += track.Metadata.TrackName;
    }

    private void InitializeComponent()
    {
    }
}