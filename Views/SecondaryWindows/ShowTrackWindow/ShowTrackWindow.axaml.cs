using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonix.Models.Media.Track;
using Avalonix.Services.WindowManager;
using Microsoft.Extensions.Logging;

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
        Name.Content = track.Metadata.TrackName;
        Artist.Content = track.Metadata.Artist;
        Album.Content = track.Metadata.Album;
        UpdateAlbumCover(track);
    }
    
    private void UpdateAlbumCover(Track track)
    {
        var coverData = track.Metadata.Cover;

        if (coverData == null || coverData.Length == 0)
        {
            Image = null;
            return;
        }

        try
        {
            using var memoryStream = new MemoryStream(coverData);
            Image = new Image
            {
                Source = new Bitmap(memoryStream),
                Stretch = Stretch.UniformToFill
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading cover: {Error}", ex.Message);
            Image = null;
        }
    }
}