using Avalonia.Controls;
using Avalonix.Model.Media.Track;
using Avalonix.Services.WindowManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.ShowTrackWindow;

public partial class ShowTrackWindow : Window
{
    public ShowTrackWindow(ILogger<WindowManager> logger, Track track)
    {
        InitializeComponent();
        logger.LogInformation("ShowTrackWindow initialized");
        Title += track.Metadata.TrackName;

        InitializeMainFields(track);
        InitializeAdditionalFields(track);
    }

    private void InitializeAdditionalFields(Track track)
    {
        Album.Text += " " + track.Metadata.Album;
        Genre.Text += " " + track.Metadata.Genre;
        MediaFileFormat.Text += " " + track.Metadata.MediaFileFormat;
        Year.Text += " " + track.Metadata.Year;
        Duration.Text += " " + TrackMetadata.ToHumanFriendlyString(track.Metadata.Duration);
    }

    private void InitializeMainFields(Track track)
    {
        Name.Text += " " + track.Metadata.TrackName;
        Artist.Text += " " + track.Metadata.Artist;
        Lyrics.Text = track.Metadata.Lyric;
    }
}