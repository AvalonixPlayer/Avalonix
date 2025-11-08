using System;
using System.IO;
using System.Linq;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
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

    private readonly Timer _timer;

    private bool _isUserDragging;

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

        InitializeComponent();

        _playlistManager.PlaybackStateChanged += UpdatePauseButtonImage;

        _playlistManager.TrackChanged += UpdateAlbumCover;
        _playlistManager.TrackChanged += UpdateSongBox;
        _playlistManager.TrackChanged += UpdateTrackPositionSlider;
        _playlistManager.TrackChanged += UpdateTrackInfo;

        _timer = new Timer(1000);
        _timer.Elapsed += UpdateTrackPositionSlider;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Start();

        SongBox.Tapped += SongBox_OnSelectionChanged;

        Dispatcher.UIThread.Post(async void () =>
            VolumeSlider.Value = (await settingsManager.GetSettings()).Avalonix.Volume);

        var trackSliderPressed = true;
        TrackPositionSlider.PointerMoved += (sender, args) => _isUserDragging = true;
        TrackPositionSlider.PointerCaptureLost += (sender, args) =>
        {
            _isUserDragging = false;
            trackSliderPressed = false;
        };
        TrackPositionSlider.PointerExited += (sender, args) =>
        {
            if (!trackSliderPressed)
                _isUserDragging = false;
        };
        TrackPositionSlider.ValueChanged += TrackPositionChange;

        _logger.LogInformation("MainWindow initialized");
    }

    protected sealed override async void OnClosed(EventArgs e) => await _windowManager.CloseMainWindowAsync();

    private async void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        await _playlistManager.ChangeVolume(Convert.ToUInt32(e.NewValue));
        _logger.LogInformation("Volume changed: {EOldValue} -> {ENewValue}", e.OldValue, e.NewValue);
    }

    private void TrackPositionChange(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
    {
        if (_isUserDragging)
            Dispatcher.UIThread.Post(async void () =>
                _playlistManager.MediaPlayer.SetPosition(TrackPositionSlider.Value));
    }

    private void UpdateTrackPositionSlider(object? sender,
        ElapsedEventArgs elapsedEventArgs)
    {
        if (!Dispatcher.UIThread.CheckAccess())
            Dispatcher.UIThread.Post(() =>
            {
                if (!_isUserDragging)
                    TrackPositionSlider.Value = _playlistManager.MediaPlayer.GetPosition();
            });
    }

    private void Pause(object sender, RoutedEventArgs e)
    {
        var playingPlaylist = _playlistManager.PlayingPlaylist;
        if (playingPlaylist == null) return;

        if (playingPlaylist.Paused)
        {
            _playlistManager.ResumePlaylist();
        }
        else
        {
            _playlistManager.PausePlaylist();
        }
    }

    private void PlayNextTrack(object sender, RoutedEventArgs e) =>
        _playlistManager.NextTrack();

    private void PlayTrackBefore(object sender, RoutedEventArgs e) =>
        _playlistManager.TrackBefore();

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.PlaylistCreateWindow_Open().ShowDialog(this);

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.PlaylistSelectWindow_Open().ShowDialog(this);

    private void UpdateSongBox()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateSongBox);
            return;
        }

        if (_playlistManager.PlayingPlaylist?.PlayQueue.Tracks == null)
        {
            _logger.LogError("Play queue is empty");
            return;
        }

        SongBox.ItemsSource = _playlistManager.PlayingPlaylist.PlayQueue.Tracks
            .Select(x => x.Metadata.TrackName).ToList();
    }

    private void UpdateTrackPositionSlider()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateTrackPositionSlider);
            return;
        }

        if (_playlistManager.MediaPlayer.CurrentTrack != null)
            TrackPositionSlider.Maximum = _playlistManager.MediaPlayer.CurrentTrack.Metadata.Duration.TotalSeconds;
        TrackPositionSlider.Value = 0;
    }

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

    private void UpdateTrackInfo()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateTrackInfo);
            return;
        }

        TrackName.Content = PostProcessedText(_playlistManager.CurrentTrack?.Metadata.TrackName, 13);
        ArtistName.Content = PostProcessedText(_playlistManager.CurrentTrack?.Metadata.Artist, 13);
    }

    private void AboutButton_OnClick(object? sender, RoutedEventArgs e) =>
        _windowManager.AboutWindow_Open().ShowDialog(this);

    private void SnuffleButton_OnClick(object? sender, RoutedEventArgs e) => _playlistManager.ResetSnuffle();

    private async void OpenTrackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var currentTrack = _playlistManager.CurrentTrack;
            if (currentTrack != null) await _windowManager.ShowTrackWindow_Open(currentTrack).ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error opening track: {Error}", ex.Message);
        }
    }

    private async void OpenEditTrackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var currentTrack = _playlistManager.CurrentTrack;
            if (currentTrack != null) await _windowManager.EditMetadataWindow_Open(currentTrack).ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error opening track edit: {Error}", ex.Message);
        }
    }

    private async void DeletePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _windowManager.PlaylistDeleteWindow_Open().ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error opening track: {Error}", ex.Message);
        }
    }

    private void SongBox_OnSelectionChanged(object? sender, TappedEventArgs tappedEventArgs)
    {
        try
        {
            var castedSender = (ListBox)sender!;
            _logger.LogInformation(castedSender.SelectedItem?.ToString());
            var selectedTrack = castedSender.SelectedItem?.ToString();
            if (selectedTrack != null)
                _playlistManager.ForceStartTrackByName(selectedTrack);
            else
                _logger.LogError("No track selected");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while force starting song: {ex}", ex.Message);
        }
    }

    private string PostProcessedText(string? enterText, int maxSymbols)
    {
        if (enterText.Length <= maxSymbols) return enterText;
        return string.Concat(enterText.AsSpan(0, maxSymbols - 3), "...");
    }
}