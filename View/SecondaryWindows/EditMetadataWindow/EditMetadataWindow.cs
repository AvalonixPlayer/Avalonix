using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Model.Media.Track;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModel.EditMetadata;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.EditMetadataWindow;

public partial class EditMetadataWindow : Window, ISecondaryWindow
{
    private readonly ILogger<WindowManager> _logger;
    private readonly IPlayablesManager _playablesManager;
    private readonly Track _track;
    private readonly IEditMetadataWindowViewModel _vm;
    private string? _newCoverPath;

    public EditMetadataWindow(ILogger<WindowManager> logger, IEditMetadataWindowViewModel vm, Track track,
        IPlayablesManager playablesManager)
    {
        _logger = logger;
        _vm = vm;
        _track = track;
        _playablesManager = playablesManager;

        InitializeComponent();
        InitializeControls();
    }

    private async void SelectCover(object? sender, RoutedEventArgs e)
    {
        _newCoverPath = await _vm.OpenTrackFileDialogAsync(this);
        _logger.LogInformation("Change cover path to {CoverPath}", _newCoverPath);
    }

    [Obsolete("Obsolete")]
    private async void Apply_OnClick(object? sender, RoutedEventArgs e)
    {
        _playablesManager.MediaPlayer.Stop();
        byte[]? cover = null;
        if (!string.IsNullOrEmpty(_newCoverPath))
            cover = File.ReadAllBytes(_newCoverPath);
        var newMetadata = new TrackMetadata
        {
            TrackName = Name.Text,
            Album = Album.Text,
            Artist = Artist.Text,
            Genre = Genre.Text,
            Year = uint.Parse(Year.Text),
            Lyric = Lyric.Text,
            Cover = cover
        };
        await _track.RewriteMetaData(newMetadata);
    }

    public void InitializeControls()
    {
        _newCoverPath = null;
        Title += _track.Metadata.TrackName;
        Name.Text = _track.Metadata.TrackName;
        Artist.Text = _track.Metadata.Artist;
        Album.Text = _track.Metadata.Album;
        Genre.Text = _track.Metadata.Genre;
        Year.Text = _track.Metadata.Year.ToString();
        Lyric.Text = _track.Metadata.Lyric;
    }
}