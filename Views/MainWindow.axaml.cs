using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonix.Models.Media.MediaPlayer;
using Avalonix.Services.PlaylistManager;
using Avalonix.Services.SettingsManager;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views;

public partial class MainWindow  : Window 
{
    private readonly ILogger<MainWindow> _logger;
    private readonly IMainWindowViewModel _vm;
    private readonly IMediaPlayer _player;
    private readonly IPlaylistManager _playlistManager;
    public MainWindow(ILogger<MainWindow> logger, IMainWindowViewModel vm, IMediaPlayer player, ISettingsManager settingsManager, IPlaylistManager playlistManager)
    {
        _logger = logger;
        _vm = vm;
        _player = player;
        _playlistManager = playlistManager;
        InitializeComponent();
        Dispatcher.UIThread.Post(async void () =>
            VolumeSlider.Value = (await settingsManager.GetSettings()).Avalonix.Volume);
        
        _logger.LogInformation("MainWindow initialized");
    }

    private async void VolumeSlider_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _logger.LogInformation($"Volume changed: {e.OldValue} -> {e.NewValue}");
        await _player.ChangeVolume((int)e.NewValue);
    }

    private void Pause(object sender, RoutedEventArgs e)
    {
        if(_playlistManager.PlayingPlaylist == null!) return;
        if(!_playlistManager.PlayingPlaylist.Paused())
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
}