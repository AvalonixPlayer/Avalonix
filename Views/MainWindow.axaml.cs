using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using Avalonia.Platform;
using Avalonix.Services.WindowManager;

namespace Avalonix.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IMainWindowViewModel _vm;
    private readonly IPlaylistManager _playlistManager;
    private readonly IWindowManager _windowManager; 

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

    public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm,
        ISettingsManager settingsManager, IPlaylistManager playlistManager, IWindowManager windowManager)
    {
        _logger = logger;
        _vm = vm;
        _playlistManager = playlistManager;
        _windowManager = windowManager;

        _playlistManager.PlaybackStateChanged += UpdatePauseButtonImage;
        _playlistManager.TrackChanged += UpdateAlbumCover;
        InitializeComponent();
        Dispatcher.UIThread.Post(async void () => VolumeSlider.Value = (await settingsManager.GetSettings()).Avalonix.Volume );
        
        _logger.LogInformation("MainWindow initialized");
    }

    protected sealed override async void OnClosed(EventArgs e) => await _windowManager.CloseMainWindowAsync();

    private async void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _logger.LogInformation("Volume changed: {EOldValue} -> {ENewValue}", e.OldValue, e.NewValue);
        await _playlistManager.ChangeVolume(Convert.ToUInt32(e.NewValue));
    }

    private void Pause(object sender, RoutedEventArgs e)
    {
        if (_playlistManager.PlayingPlaylist == null!) return;
        if (!_playlistManager.PlayingPlaylist.Paused)
            _playlistManager.PausePlaylist();
        else
            _playlistManager.ResumePlaylist();
    }

    private void PlayNextTrack(object sender, RoutedEventArgs e) =>
        _playlistManager.NextTrack();

    private void PlayTrackBefore(object sender, RoutedEventArgs e) =>
        _playlistManager.TrackBefore();

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.PlaylistCreateWindow_Open().ShowDialog(this);

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.PlaylistSelectWindow_Open().ShowDialog(this);

    private void UpdatePauseButtonImage(bool pause)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => UpdatePauseButtonImage(pause));
            return;
        }
        PauseButton.Content = pause ? _playButtonImage : _pauseButtonImage;
    }

    private void UpdateAlbumCover()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateAlbumCover);
            return;
        }

        var currentTrack = _playlistManager.CurrentTrack; 

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

    private void AboutButton_OnClick(object? sender, RoutedEventArgs e) => _windowManager.AboutWindow_Open().ShowDialog(this);
}