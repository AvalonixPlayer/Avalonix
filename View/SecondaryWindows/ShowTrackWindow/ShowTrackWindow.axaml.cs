using Avalonia.Controls;
using Avalonix.Model.Media.Track;
using Avalonix.Services.WindowManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.ShowTrackWindow;

public partial class ShowTrackWindow : Window, ISecondaryWindow
{
    private readonly Track _track;
    public ShowTrackWindow(ILogger<WindowManager> logger, Track track)
    {
        InitializeComponent();
        logger.LogInformation("ShowTrackWindow initialized");
        _track = track;
        InitializeControls();
    }

    public void InitializeControls()
    {
        Title += _track.Metadata.TrackName;
        Name.Text += " " + _track.Metadata.TrackName;
        Artist.Text += " " + _track.Metadata.Artist;
        Lyrics.Text = _track.Metadata.Lyric;
        Album.Text += " " + _track.Metadata.Album;
        Genre.Text += " " + _track.Metadata.Genre;
        MediaFileFormat.Text += " " + _track.Metadata.MediaFileFormat;
        Year.Text += " " + _track.Metadata.Year;
        Duration.Text += " " + TrackMetadata.ToHumanFriendlyString(_track.Metadata.Duration);
    }
}