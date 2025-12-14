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
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonix.Model.Media.Track;
using Avalonix.Services.CacheManager;
using Avalonix.Services.PlayableManager;
using Avalonix.Services.PlayableManager.PlayboxManager;
using Avalonix.Services.SettingsManager;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModel.Main;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Avalonix.View;

public partial class MainWindow : Window
{
    private readonly Image _disableLoopImage = GetImageFromAvares("buttons/DisableLoop.png");

    private readonly Image _disableShuffleImage = GetImageFromAvares("buttons/DisableSnuffle.png");

    private readonly Image _enableLoopImage = GetImageFromAvares("buttons/EnableLoop.png");

    private readonly Image _enableShuffleImage = GetImageFromAvares("buttons/EnableSnuffle.png");
    private readonly ILogger<MainWindow> _logger;

    private readonly Image _pauseButtonImage = GetImageFromAvares("buttons/pause.png");
    private readonly IPlayablesManager _playablesManager;
    private readonly IPlayboxManager _playboxManager;

    private readonly Image _playButtonImage = GetImageFromAvares("buttons/play.png");

    private readonly Timer _timer;
    private readonly IMainWindowViewModel _vm;
    private readonly IWindowManager _windowManager;
    private readonly ISettingsManager _settingsManager;
    private readonly ICacheManager _cacheManager;

    private bool _isUserDragging;

    public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm,
        ISettingsManager settingsManager, ICacheManager cacheManager, IPlayablesManager playablesManager,
        IWindowManager windowManager,
        IPlayboxManager playboxManager)
    {
        _logger = logger;
        _vm = vm;
        _playablesManager = playablesManager;
        _windowManager = windowManager;
        _playboxManager = playboxManager;
        _settingsManager = settingsManager;
        _cacheManager = cacheManager;

        InitializeComponent();

        _playablesManager.PlaybackStateChanged += UpdatePauseButtonImage;
        SubscribePlayableChanged();
        SubscribeTrackChanged();

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

    private void SelectTab(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tabName }) return;

        PlaylistsContent.IsVisible = false;
        PlayQueueContent.IsVisible = false;
        MetadataTabContent.IsVisible = false;
        SettingsTabContent.IsVisible = false;

        switch (tabName)
        {
            case "PlayboxesTab":
                PlayboxesTabContent.IsVisible = true;
                MetadataTabContent.IsVisible = false;
                SettingsTabContent.IsVisible = false;
                break;
            case "MetadataTab":
                PlayboxesTabContent.IsVisible = false;
                MetadataTabContent.IsVisible = true;
                SettingsTabContent.IsVisible = false;
                break;
            case "SettingsTab":
                PlayboxesTabContent.IsVisible = false;
                MetadataTabContent.IsVisible = false;
                SettingsTabContent.IsVisible = true;
                break;
            case "PlaylistsTab":
                PlaylistsContent.IsVisible = true;
                break;
            case "PlayQueueTab":
                PlayQueueContent.IsVisible = true;
                break;
        }
    }

    private void DisableTabs()
    {
        PlaylistsContent.IsVisible = false;
        PlayQueueContent.IsVisible = false;
        MetadataTabContent.IsVisible = false;
        SettingsTabContent.IsVisible = false;
        GlobalTabs.IsVisible = false;

        PlayboxesTabContent.IsVisible = false;
        MetadataTabContent.IsVisible = false;
        SettingsTabContent.IsVisible = false;
    }

    private void SetRightPartVisible(int index)
    {
        MainWindowPlayerButtons.IsVisible = index > 0;
        VolumeSlider.IsVisible = index > 1;
        TrackName.IsVisible = index > 2;
        ArtistName.IsVisible = index > 3;
        TrackPositionSlider.IsVisible = index > 4;
        TrackDuration.IsVisible = index > 5;
    }

    private void PlayAllTracks_OnClick(object? sender, RoutedEventArgs e)
    {
        _playablesManager.StartPlayable(_playboxManager.GetPlayables().Result[0]);
    }

    private async void NewPlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.PlaylistCreateWindow_Open().ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while create playlist: {ex}", ex);
        }
    }

    private async void SelectPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.PlaylistSelectWindow_Open().ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while select playlist: {ex}", ex);
        }
    }

    private async void SelectAlbum_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _vm.AlbumSelectWindow_Open().ShowDialog(this);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while select album: {ex}", ex);
        }
    }

    private void PauseButton_OnClick(object sender, RoutedEventArgs e)
    {
        var playingPlaylist = _playablesManager.PlayingPlayable;
        if (playingPlaylist == null) return;

        if (playingPlaylist.PlayQueue.Paused)
            _playablesManager.ResumePlayable();
        else
            _playablesManager.PausePlayable();
    }

    private void PlayNextTrack(object sender, RoutedEventArgs e)
    {
        _playablesManager.NextTrack();
    }

    private void PlayTrackBefore_OnClick(object sender, RoutedEventArgs e)
    {
        _playablesManager.TrackBefore();
    }

    private void AboutButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _windowManager.AboutWindow_Open().ShowDialog(this);
    }

    private void ShuffleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _playablesManager.ResetSnuffle();
    }

    private void LoopButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _playablesManager.ResetLoop();
    }

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

    private async void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        await _playablesManager.ChangeVolume(Convert.ToUInt32(e.NewValue));
        _logger.LogInformation("Volume changed: {EOldValue} -> {ENewValue}", e.OldValue, e.NewValue);
    }

    private void TrackPositionChange(object? sender, RangeBaseValueChangedEventArgs rangeBaseValueChangedEventArgs)
    {
        if (_isUserDragging)
            Dispatcher.UIThread.Post(void () =>
            {
                _playablesManager.MediaPlayer.SetPosition(TrackPositionSlider.Value);
                UpdateTrackTime();
            });
    }

    private void UpdateTrackTime()
    {
        if (_playablesManager.MediaPlayer.CurrentTrack == null) return;
        TrackDuration.Content = PostProcessedText(
            TrackMetadata.ToHumanFriendlyString(TimeSpan.FromSeconds(_playablesManager.MediaPlayer.GetPosition())),
            25) + @"/" + PostProcessedText(
            TrackMetadata.ToHumanFriendlyString(_playablesManager.CurrentTrack!.Metadata.Duration),
            25);
    }

    private void UpdateTrackPositionSlider(object? sender,
        ElapsedEventArgs elapsedEventArgs)
    {
        if (!Dispatcher.UIThread.CheckAccess())
            Dispatcher.UIThread.Post(() =>
            {
                if (!_isUserDragging)
                    TrackPositionSlider.Value = _playablesManager.MediaPlayer.GetPosition();
                UpdateTrackTime();
            });
    }

    private void SubscribePlayableChanged()
    {
        _playablesManager.PlayableChanged += SubscribeTrackMetadataLoaded;
        _playablesManager.PlayableChanged += UpdateSongBox;
    }

    private void SubscribeTrackChanged()
    {
        _playablesManager.TrackChanged += UpdateAlbumCover;
        _playablesManager.TrackChanged += UpdateSongBox;
        _playablesManager.TrackChanged += UpdateTrackPositionSlider;
        _playablesManager.TrackChanged += UpdateTrackInfo;
    }

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
            .Where(track => !string.IsNullOrEmpty(track.Metadata.TrackName)).ToList()
            .Select(track => PostProcessedText(track.Metadata.TrackName, 20));
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
            _ = Task.Run(() =>
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
            });
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

        if (_playablesManager.CurrentTrack == null)
        {
            _logger.LogError("Current track is null");
            return;
        }

        TrackName.Content = PostProcessedText(_playablesManager.CurrentTrack?.Metadata.TrackName, 25);
        ArtistName.Content = PostProcessedText(_playablesManager.CurrentTrack?.Metadata.Artist, 25);
        TrackDuration.Content =
            PostProcessedText(TrackMetadata.ToHumanFriendlyString(_playablesManager.CurrentTrack!.Metadata.Duration),
                25);
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

    private static Image GetImageFromAvares(string partOfPath)
    {
        return new Image
        {
            Source =
                new Bitmap(AssetLoader.Open(new Uri($"avares://Avalonix/Assets/{partOfPath}"))),
            Width = 35,
            Height = 35,
            Stretch = Stretch.Uniform
        };
    }

    protected sealed override async void OnClosing(WindowClosingEventArgs e)
    {
        await Task.Run(() =>
        {
            _settingsManager.SaveSettingsAsync().GetAwaiter().GetResult();
            _cacheManager.SaveCacheAsync().GetAwaiter().GetResult();
        });
    }

    private async void OpenSettingsWindowButton_OnClick(object? sender, RoutedEventArgs e) =>
        await _windowManager.SettingsWindow_Open().ShowDialog(this);

    private void MainWindow_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var height = e.NewSize.Height;

        switch (height)
        {
            case <= 400:
                SetRightPartVisible(0);
                break;
            case <= 430:
                SetRightPartVisible(1);
                break;
            case <= 450:
                SetRightPartVisible(2);
                break;
            case <= 500:
                SetRightPartVisible(3);
                break;
            case <= 550:
                SetRightPartVisible(4);
                break;
            case <= 600:
                SetRightPartVisible(5);
                break;
            default:
                SetRightPartVisible(6);
                break;
        }

        if (e.NewSize.Width <= 610)
        {
            var mainWindowTrackInfo = this.FindControl<Border>("MainWindowTrackInfo")!;
            var mainWindowPlayerButtons = this.FindControl<Border>("MainWindowPlayerButtons")!;
            mainWindowTrackInfo.SetValue(Grid.ColumnProperty, 0);
            mainWindowPlayerButtons.SetValue(Grid.ColumnProperty, 0);
            DisableTabs();
        }
        else
        {
            var mainWindowTrackInfo = this.FindControl<Border>("MainWindowTrackInfo")!;
            var mainWindowPlayerButtons = this.FindControl<Border>("MainWindowPlayerButtons")!;
            mainWindowTrackInfo.SetValue(Grid.ColumnProperty, 1);
            mainWindowPlayerButtons.SetValue(Grid.ColumnProperty, 1);
            GlobalTabs.IsVisible = true;
        }

        _logger.LogInformation("Window size changed : Width: {width}, Height: {height}", e.NewSize.Width,
            e.NewSize.Height);
    }
}