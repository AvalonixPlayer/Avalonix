using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using Avalonia.Platform;

namespace Avalonix.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IMainWindowViewModel _vm;
    private readonly IPlaylistManager _playlistManager;
    
    private readonly DispatcherTimer _timer;
    
    private string? _currentTrackPath;

    private readonly Image _playButtonImage = new()
    {
        Source =
            new Bitmap(AssetLoader.Open(new Uri("avares://Avalonix/Assets/buttons/play.png")))
    };

    private readonly Image _pauseButtonImage = new()
    {
        Source =
            new Bitmap(AssetLoader.Open(new Uri("avares://Avalonix/Assets/buttons/pause.png")))
    };

    public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm, IMediaPlayer player,
        ISettingsManager settingsManager, IPlaylistManager playlistManager)
    {
        _logger = logger;
        _vm = vm;
        _playlistManager = playlistManager;
        InitializeComponent();
        Dispatcher.UIThread.Post(async void () => VolumeSlider.Value = (await settingsManager.GetSettings()).Avalonix.Volume );
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };

        _timer.Tick += UpdatePauseButtonImage;
        _timer.Tick += UpdateAlbumCover;
        _timer.Start();
        
        _logger.LogInformation("MainWindow initialized");
    }

    private async void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _logger.LogInformation("Volume changed: {EOldValue} -> {ENewValue}", e.OldValue, e.NewValue);
        await _playlistManager.ChangeVolume(Convert.ToUInt32(e.NewValue));
    }

    private void Pause(object sender, RoutedEventArgs e)
    {
        if (_playlistManager.PlayingPlaylist == null!) return;
        if (!_playlistManager.PlayingPlaylist.Paused())
            _playlistManager.PausePlaylist();
        else
            _playlistManager.ResumePlaylist();
    }

    private void PlayNextTrack(object sender, RoutedEventArgs e) =>
        _playlistManager.NextTrack();

    private void PlayTrackBefore(object sender, RoutedEventArgs e) =>
        _playlistManager.TrackBefore();

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e) =>
        await (await _vm.PlaylistCreateWindow_Open()).ShowDialog(this);

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e) =>
        await (await _vm.PlaylistSelectWindow_Open()).ShowDialog(this);

    private void UpdatePauseButtonImage(object? sender, EventArgs e) =>
        PauseButton.Content = !_playlistManager.IsPaused ? _pauseButtonImage : _playButtonImage;

    private void UpdateAlbumCover(object? sender, EventArgs e)
    {
        var currentTrack = _playlistManager.CurrentTrack;
        var currentPath = currentTrack?.TrackData.Path;
        
        if (_currentTrackPath == currentPath)
            return;

        _currentTrackPath = currentPath;

        var coverData = currentTrack?.Metadata.Cover;
    
        if (coverData == null || coverData.Length == 0)
        {
            AlbumCover.Child = null;
            return;
        }

        try
        {
            using var memoryStream = new MemoryStream(coverData);
            AlbumCover.Child = new Image 
            { 
                Source = new Bitmap(memoryStream),
                Stretch = Stretch.UniformToFill
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading cover: {Error}", ex.Message);
            AlbumCover.Child = null;
        }
    }
}