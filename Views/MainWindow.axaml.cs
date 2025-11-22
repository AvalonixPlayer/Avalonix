using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.PlaylistManager;
using Avalonix.Services.WindowManager;
using Timer = System.Timers.Timer;

namespace Avalonix.Views;

public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IMainWindowViewModel _vm;
    private readonly IPlayablesManager _playablesManager;
    private readonly IWindowManager _windowManager;

    private readonly Timer _timer;

    private bool _isUserDragging;

    private readonly Image _playButtonImage = GetImageFromAvares("buttons/play.png");

    private readonly Image _pauseButtonImage = GetImageFromAvares("buttons/pause.png");

    private readonly Image _enableShuffleImage = GetImageFromAvares("buttons/EnableSnuffle.png");

    private readonly Image _disableShuffleImage = GetImageFromAvares("buttons/DisableSnuffle.png");

    private readonly Image _enableLoopImage = GetImageFromAvares("buttons/EnableLoop.png");

    private readonly Image _disableLoopImage = GetImageFromAvares("buttons/DisableLoop.png");

    public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm,
        ISettingsManager settingsManager, IPlayablesManager playablesManager, IWindowManager windowManager)
    {
        _logger = logger;
        _vm = vm;
        _playablesManager = playablesManager;
        _windowManager = windowManager;

        InitializeComponent();

        _playablesManager.PlaybackStateChanged += UpdatePauseButtonImage;
        _playablesManager.PlayableChanged += SubscribeTrackMetadataLoaded;
        _playablesManager.PlayableChanged += UpdateSongBox;

        _playablesManager.TrackChanged += UpdateAlbumCover;
        _playablesManager.TrackChanged += UpdateSongBox;
        _playablesManager.TrackChanged += UpdateTrackPositionSlider;
        _playablesManager.TrackChanged += UpdateTrackInfo;
        _playablesManager.ShuffleChanged += UpdateShuffleButtonImage;
        _playablesManager.LoopChanged += UpdateLoopButtonImage;

        UpdateShuffleButtonImage(settingsManager.Settings!.Avalonix.PlaySettings.Shuffle);
        UpdateLoopButtonImage(settingsManager.Settings!.Avalonix.PlaySettings.Loop);

        _timer = new Timer(1000);
        _timer.Elapsed += UpdateTrackPositionSlider;
        _timer.AutoReset = true;
        _timer.Enabled = true;
        _timer.Start();

        SongBox.Tapped += SongBox_OnSelectionChanged;

        Dispatcher.UIThread.Post(void () =>
            VolumeSlider.Value = settingsManager.Settings!.Avalonix.Volume);

        var trackSliderPressed = false;
        TrackPositionSlider.PointerMoved += (_, _) => _isUserDragging = true;
        TrackPositionSlider.PointerCaptureLost += (_, _) =>
        {
            _isUserDragging = false;
            trackSliderPressed = false;
        };
        TrackPositionSlider.PointerExited += (_, _) =>
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
        await _playablesManager.ChangeVolume(Convert.ToUInt32(e.NewValue));
        _logger.LogInformation("Volume changed: {EOldValue} -> {ENewValue}", e.OldValue, e.NewValue);
    }

    private void TrackPositionChange(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
    {
        if (_isUserDragging)
            Dispatcher.UIThread.Post(async void () =>
                _playablesManager.MediaPlayer.SetPosition(TrackPositionSlider.Value));
    }

    private void UpdateTrackPositionSlider(object? sender,
        ElapsedEventArgs elapsedEventArgs)
    {
        if (!Dispatcher.UIThread.CheckAccess())
            Dispatcher.UIThread.Post(() =>
            {
                if (!_isUserDragging)
                    TrackPositionSlider.Value = _playablesManager.MediaPlayer.GetPosition();
            });
    }

    private void Pause(object sender, RoutedEventArgs e)
    {
        var playingPlaylist = _playablesManager.PlayingPlayable;
        if (playingPlaylist == null) return;

        if (playingPlaylist.PlayQueue.Paused)
            _playablesManager.ResumePlayable();
        else
            _playablesManager.PausePlayable();
    }

    private void PlayNextTrack(object sender, RoutedEventArgs e) =>
        _playablesManager.NextTrack();

    private void PlayTrackBefore(object sender, RoutedEventArgs e) =>
        _playablesManager.TrackBefore();

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.PlaylistCreateWindow_Open().ShowDialog(this);

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.PlaylistSelectWindow_Open().ShowDialog(this);
    
    private async void SelectAlbum_OnClick(object? sender, RoutedEventArgs e) =>
        await _vm.AlbumSelectWindow_Open().ShowDialog(this);

    private void SubscribeTrackMetadataLoaded()
    {
        if (_playablesManager.PlayingPlayable == null) return;

        for (var i = 0; i < _playablesManager.PlayingPlayable!.PlayQueue.Tracks.Count; i++)
        {
            _playablesManager.PlayingPlayable.PlayQueue.Tracks[i].Metadata.MetadataLoaded += UpdateSongBox;
            var i1 = i;
            _playablesManager.PlayingPlayable.PlayQueue.Tracks[i].Metadata.MetadataLoaded += () =>
            {
                if (i1 == _playablesManager.PlayingPlayable.PlayQueue.PlayingIndex)
                    UpdateAlbumCover();
            };
        }
    }

    private void UpdateSongBox()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateSongBox);
            return;
        }

        if (_playablesManager.PlayingPlayable?.QueueIsEmpty() == true)
        {
            _logger.LogError("Play queue is empty");
            return;
        }

        SongBox.ItemsSource = _playablesManager.PlayingPlayable?.PlayQueue.Tracks
            .Select(x => PostProcessedText(x.Metadata.TrackName, 30)).ToList();
    }

    private void UpdateTrackPositionSlider()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(UpdateTrackPositionSlider);
            return;
        }

        if (_playablesManager.MediaPlayer.CurrentTrack != null)
            TrackPositionSlider.Maximum = _playablesManager.MediaPlayer.CurrentTrack.Metadata.Duration.TotalSeconds;
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

        var currentTrack = _playablesManager.CurrentTrack;

        var coverData = currentTrack?.Metadata.Cover;

        if (coverData == null || coverData.Length == 0)
        {
            AlbumCover.Child = null;
            return;
        }

        try
        {
            new Task(() =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    using var memoryStream = new MemoryStream(coverData);
                    AlbumCover.Child = new Image
                    {
                        Source = new Bitmap(memoryStream),
                        Stretch = Stretch.UniformToFill
                    };
                });
            }).Start();
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

        TrackName.Content = PostProcessedText(_playablesManager.CurrentTrack?.Metadata.TrackName, 13);
        ArtistName.Content = PostProcessedText(_playablesManager.CurrentTrack?.Metadata.Artist, 13);
    }

    private void AboutButton_OnClick(object? sender, RoutedEventArgs e) =>
        _windowManager.AboutWindow_Open().ShowDialog(this);

    private void ShuffleButton_OnClick(object? sender, RoutedEventArgs e) =>
        _playablesManager.ResetSnuffle();

    private void LoopButton_OnClick(object? sender, RoutedEventArgs e) =>
        _playablesManager.ResetLoop();

    private void UpdateShuffleButtonImage(bool enable)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => UpdateShuffleButtonImage(enable));
            return;
        }

        Shuffle.Content = !enable ? _enableShuffleImage : _disableShuffleImage;
    }

    private void UpdateLoopButtonImage(bool enable)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => UpdateLoopButtonImage(enable));
            return;
        }

        Loop.Content = !enable ? _enableLoopImage : _disableLoopImage;
    }

    private async void OpenTrackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var currentTrack = _playablesManager.CurrentTrack;
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
            var currentTrack = _playablesManager.CurrentTrack;
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
            _logger.LogInformation(castedSender.SelectedIndex.ToString());
            var selectedIndex = castedSender.SelectedIndex;
            var selectedTrack = _playablesManager.PlayingPlayable?.PlayQueue.Tracks[selectedIndex];
            if (selectedTrack != null)
                _playablesManager.ForceStartTrackByIndex(selectedIndex);
            else
                _logger.LogError("No track selected");
            castedSender.Selection = null!;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while force starting song: {ex}", ex.Message);
        }
    }

    private string PostProcessedText(string? enterText, int maxSymbols)
    {
        try
        {
            if (string.IsNullOrEmpty(enterText)) return string.Empty;
            return enterText.Length <= maxSymbols
                ? enterText
                : string.Concat(enterText.AsSpan(0, maxSymbols - 3), "...");
        }
        catch (Exception e)
        {
            _logger.LogError("Error post process text: {mess}", e.Message);
            return enterText ?? string.Empty;
        }
    }

    private static Image GetImageFromAvares(string partOfPath) =>
        new()
        {
            Source =
                new Bitmap(AssetLoader.Open(new Uri($"avares://Avalonix/Assets/{partOfPath}")))
        };
}