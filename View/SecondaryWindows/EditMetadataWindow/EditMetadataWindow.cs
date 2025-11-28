using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonix.Model.Media.Track;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModel.EditMetadata;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.EditMetadataWindow;

public partial class EditMetadataWindow : Window
{
    private readonly ILogger<WindowManager> _logger;
    private readonly IPlaylistManager _playlistManager;
    private readonly Track _track;
    private readonly IEditMetadataWindowViewModel _vm;
    private string? _newCoverPath;

    public EditMetadataWindow(ILogger<WindowManager> logger, IEditMetadataWindowViewModel vm, Track track,
        IPlaylistManager playlistManager)
    {
        _logger = logger;
        _vm = vm;
        _track = track;
        _playlistManager = playlistManager;

        InitializeComponent();
        InitializeFields(track);
    }

    private void InitializeFields(Track track)
    {
        _newCoverPath = null;
        Name.Text = track.Metadata.TrackName;
        Artist.Text = track.Metadata.Artist;
        Album.Text = track.Metadata.Album;
        Genre.Text = track.Metadata.Genre;
        Year.Text = track.Metadata.Year.ToString();
        Lyric.Text = track.Metadata.Lyric;
    }

    private async void SelectCover(object? sender, RoutedEventArgs e)
    {
        _newCoverPath = await _vm.OpenTrackFileDialogAsync(this);
        _logger.LogInformation("Change cover path to {CoverPath}", _newCoverPath);
    }

    [Obsolete("Obsolete")]
    private void Apply_OnClick(object? sender, RoutedEventArgs e)
    {
        _playlistManager.MediaPlayer.Stop();
        byte[]? cover = null;
        if (!string.IsNullOrEmpty(_newCoverPath))
            cover = File.ReadAllBytes(_newCoverPath);
        new Task(() =>
            Dispatcher.UIThread.Post(() => _track.Metadata.RewriteTags(_track.TrackData.Path, Name.Text!, Album.Text!,
                Artist.Text!,
                Genre.Text!, int.Parse(Year.Text!),
                Lyric.Text!, cover))).Start();
    }
}