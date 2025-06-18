using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using AvalonixAPI;
using static Avalonix.Program;
using System.Timers;
using Avalonia.Threading;
using Avalonix.AvalonixAPI;
using NAudio.Wave;

namespace Avalonix;

public partial class MainWindow : Window
{
    private const string CurrentPlaylistName = "TestPlaylist";
    public required Timer? PlaybackTimer;
    private readonly TextBlock? _playbackTimeTextBlock;
    private readonly Button? _forwardButton;

    readonly string[] _supportedAudioFormats = [
        "*.mp3", "*.wav", "*.flac", "*.opus", 
        "*.aiff", "*.aif", "*.wma", "*.asf", 
        "*.ac3", "*.adts", "*.mp2", "*.mpa", 
        "*.m4a", "*.mp4", "*.ogg", "*.aac", 
        "*.adt", "*.dts", "*.ape", "*.wv", 
        "*.caf", "*.gsm", "*.alaw", "*.ulaw", 
        "*.dwd", "*.snd", "*.au", "*.raw", 
        "*.pcm"
    ];

    public MainWindow()
    {
        Logger.Info("MainWindow opened");
        try
        {
            _playbackTimeTextBlock = this.FindControl<TextBlock>("TimeSong");
            _forwardButton = this.FindControl<Button>("ForwardButton");
        }
        catch (Exception e)
        {
           Logger.Fatal(e.Message); 
        }
        
        InitializeComponent();
        InitializePlaybackTimer();
        UpdatePlaylistBox();
    }

    private void InitializePlaybackTimer()
    {
        PlaybackTimer = new Timer(1000); 
        PlaybackTimer.Elapsed += UpdatePlaybackTime;
        PlaybackTimer.AutoReset = true;
    }

    private void UpdatePlaybackTime(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (MediaPlayer.State != PlaybackState.Playing) return;
            var current = MediaPlayer.PlaybackTime;
            var total = MediaPlayer.TotalMusicTime;
            Debug.Assert(_playbackTimeTextBlock != null, nameof(_playbackTimeTextBlock) + " != null");
            _playbackTimeTextBlock.Text = $@"{current:mm\:ss} / {total:mm\:ss}";
        });
    }

    private void UpdatePlaylistBox()
    {
        PlaylistBox.Items.Clear();
        try
        {
            var songNames = PlaylistsManager.SongsNamesInPlaylist(CurrentPlaylistName);
            foreach (var song in songNames)
            {
                PlaylistBox.Items.Add(new ListBoxItem { Content = song });
            }
            Logger.Info($"Loaded songs for playlist: {CurrentPlaylistName}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load playlist {CurrentPlaylistName}: {ex.Message}");
        }
    }

    private async void AddSongButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Info("Add song button clicked");
            var topLevel = GetTopLevel(this);
            var files = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Audio File",
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Audio Files") { Patterns = _supportedAudioFormats }]
            })!;

            if (!(files.Count > 0)) return;
            try
            {
                var filePath = files[0].Path.LocalPath;
                var songTitle = System.IO.Path.GetFileNameWithoutExtension(filePath);
                var songData = new SongData(songTitle, filePath);
                PlaylistsManager.AddSongToPlaylist(CurrentPlaylistName, songData);
                UpdatePlaylistBox();
                Logger.Info($"Added song {songTitle} to playlist {CurrentPlaylistName}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to add song: {ex.Message}");
            }
        }
        catch (Exception x)
        {
            Logger.Error(x.Message);
        }
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Remove button clicked");
        if (PlaylistBox.SelectedItem is not ListBoxItem selectedItem) return;
        try
        {
            var songTitle = selectedItem.Content?.ToString();
            if (songTitle == null) return;
            PlaylistsManager.RemoveSongFromPlaylist(CurrentPlaylistName, songTitle);
            UpdatePlaylistBox();
            Logger.Info($"Removed song {songTitle} from playlist {CurrentPlaylistName}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to remove song: {ex.Message}");
        }
    }

    private void PauseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Play/Pause button clicked");
        try
        {
            if (MediaPlayer.State == PlaybackState.Playing)
            {
                PlaylistsManager.PausePlaylist();
                PlaybackTimer?.Stop();
                Debug.Assert(_forwardButton != null, nameof(_forwardButton) + " != null");
                _forwardButton.Content = "⏯"; 
                Logger.Info("Playback paused");
            }
            else
            {
                if (PlaylistBox.SelectedItem is ListBoxItem selectedItem)
                {
                    if (selectedItem.Content == null) return;
                    var songTitle = selectedItem.Content.ToString();
                    var playlist = PlaylistsManager.JsonToPlaylist(
                        System.IO.Path.Combine(DiskManager.SettingsPath, $"{CurrentPlaylistName}.json"));
                    var song = playlist.Songs.FirstOrDefault(s => s.Title == songTitle);
                    if (song == null) return;
                    MediaPlayer.Play(song.FilePath);
                    PlaybackTimer?.Start();
                    Debug.Assert(_forwardButton != null, nameof(_forwardButton) + " != null");
                    _forwardButton.Content = "⏸"; 
                    Logger.Info($"Playing song: {songTitle}");
                }
                else
                {
                    PlaylistsManager.PlayPlaylist(CurrentPlaylistName);
                    PlaybackTimer?.Start();
                    Debug.Assert(_forwardButton != null, nameof(_forwardButton) + " != null");
                    _forwardButton.Content = "⏸"; 
                    Logger.Info($"Playing playlist: {CurrentPlaylistName}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Playback error: {ex.Message}");
        }
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Previous button clicked");
        try
        {
            if (PlaylistBox.SelectedIndex <= 0) return;
            PlaylistBox.SelectedIndex--;
            PlaySelectedSong();
        }
        catch (Exception ex)
        {
            Logger.Error($"Previous song error: {ex.Message}");
        }
    }

    private void ForwardButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Logger.Info("Next button clicked");
        try
        {
            if (PlaylistBox.SelectedIndex >= PlaylistBox.Items.Count - 1) return;
            PlaylistBox.SelectedIndex++;
            PlaySelectedSong();
        }
        catch (Exception ex)
        {
            Logger.Error($"Next song error: {ex.Message}");
        }
    }

    private void AvaloniaObject_OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != "Value" || sender is not Slider { Name: null } slider) return;
        try
        {
            float volume = (float)(slider.Value / 100.0);
            MediaPlayer.ChangeVolume(volume);
            Logger.Debug($"Volume changed to {volume}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Volume change error: {ex.Message}");
        }
    }

    private void PlaySelectedSong()
    {
        if (PlaylistBox.SelectedItem is not ListBoxItem selectedItem) return;
        try
        {
            var songTitle = selectedItem.Content?.ToString();
            var playlist = PlaylistsManager.JsonToPlaylist(
                System.IO.Path.Combine(DiskManager.SettingsPath, $"{CurrentPlaylistName}.json"));
            var song = playlist.Songs.FirstOrDefault(s => s.Title == songTitle);
            if (song == null) return;
            MediaPlayer.Play(song.FilePath);
            PlaybackTimer?.Start();
            Debug.Assert(_forwardButton != null, nameof(_forwardButton) + " != null");
            _forwardButton.Content = "⏸"; 
            Logger.Info($"Playing song: {songTitle}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Play song error: {ex.Message}");
        }
    }
}