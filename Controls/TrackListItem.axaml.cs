using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Models.Media.Track;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.WindowManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Controls;

public partial class TrackListItem : ListBoxItem
{
    private readonly IWindowManager _windowManager;
    private readonly ILogger _logger;
    private readonly IPlaylistManager _playlistManager;
    public TrackListItem(ILogger logger, IWindowManager windowManager, IPlaylistManager playlistManager)
    {
        _logger = logger;
        _windowManager = windowManager;
        _playlistManager = playlistManager;
        InitializeComponent();
    }


    private void RemoveTrack_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            try
            {
                var castedSender = (TrackListItem)sender;
                var pl = _playlistManager.PlayingPlaylist;

                pl?.RemoveTrack(GetTrack(sender));
            }
            catch (Exception exception)
            {
                _logger.LogError("Error while removing track: {ex}", exception);
            }
        }
        else
        {
            _logger.LogError("Error while casting ListItem in TrackListItem");
        }
    }


    private void ShowTrack_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            try
            {
                var track = GetTrack(sender);

                if (track != null) _windowManager.ShowTrackWindow_Open(track);
            }
            catch (Exception exception)
            {
                _logger.LogError("Error while showing track: {ex}", exception);
            }
        }
        else
        {
            _logger.LogError("Error while casting ListItem in TrackListItem");
        }
    }

    private Track? GetTrack(object? sender)
    {
        var castedSender = (TrackListItem)sender!;
        var pl = _playlistManager.PlayingPlaylist;
        var tracks = pl?.PlaylistData.Tracks;
        return tracks!.FirstOrDefault(tr => ReferenceEquals(tr.Metadata.TrackName, castedSender.Content));
    }
}

