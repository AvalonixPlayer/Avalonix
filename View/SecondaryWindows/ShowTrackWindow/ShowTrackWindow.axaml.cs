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
        Album.Content += " " + track.Metadata.Album;
        Genre.Content += " " + track.Metadata.Genre;
        MediaFileFormat.Content += " " + track.Metadata.MediaFileFormat;
        Year.Content += " " + track.Metadata.Year;
        Duration.Content += " " + TrackMetadata.ToHumanFriendlyString(track.Metadata.Duration);
    }

    private void InitializeMainFields(Track track)
    {
        Name.Content += " " + track.Metadata.TrackName;
        Artist.Content += " " + track.Metadata.Artist;
        Lyrics.Text = track.Metadata.Lyric;
    }
}