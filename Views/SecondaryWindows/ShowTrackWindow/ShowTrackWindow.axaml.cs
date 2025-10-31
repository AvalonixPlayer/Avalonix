using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonix.Models.Media.Track;
using Avalonix.Services.WindowManager;
using Microsoft.Extensions.Logging;
using static Avalonia.Threading.Dispatcher;

namespace Avalonix.Views.SecondaryWindows.ShowTrackWindow;

public partial class ShowTrackWindow : Window
{
    private readonly ILogger<WindowManager> _logger;
    public ShowTrackWindow(ILogger<WindowManager> logger, Track track)
    {
        _logger = logger;
        InitializeComponent();   
        logger.LogInformation("ShowTrackWindow initialized");
        Title += track.Metadata.TrackName;
        
        UpdateAlbumCover(track);
        InitializeMainFields(track);
        InitializeAdditionalFields(track);
    }

    private void InitializeAdditionalFields(Track track)
    {
        Album.Content += track.Metadata.Album;
        Genre.Content += track.Metadata.Genre;
        MediaFileFormat.Content += track.Metadata.MediaFileFormat;
        Year.Content += track.Metadata.Year.ToString();
        Duration.Content += track.Metadata.Duration.Seconds + "s";
    }

    private void InitializeMainFields(Track track)
    {
        Name.Content += track.Metadata.TrackName;
        Artist.Content += track.Metadata.Artist;
        Lyrics.Content = track.Metadata.Lyric;
    }
    
    private void UpdateAlbumCover(Track track)
    {
        var coverData = track.Metadata.Cover;
        
        if (!UIThread.CheckAccess())
        {
            UIThread.Post(() => UpdateAlbumCover(track));
            return;
        }
        
        if (coverData == null || coverData.Length == 0)
        {
            Cover = null;
            return;
        }

        try
        {
            using var memoryStream = new MemoryStream(coverData);
            Cover = new Image
            {
                Source = new Bitmap(memoryStream),
                Stretch = Stretch.UniformToFill
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading cover: {Error}", ex.Message);
            Cover = null;
        }
    }
}