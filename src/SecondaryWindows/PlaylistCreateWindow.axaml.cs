using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonix.AvalonixAPI;
using static Avalonix.Program;

namespace Avalonix.SecondaryWindows;

public partial class PlaylistCreateWindow : Window
{
    private readonly TextBox _playlistNameTextBox;
    private readonly Button _songRemoveButton;
    private readonly ListBox _playlistBox;
    private readonly List<SongData> _songs = [];
    
    private readonly string[] _supportedAudioFormats = [
        "*.mp3", "*.wav", "*.flac", "*.opus", 
        "*.aiff", "*.aif", "*.wma", "*.asf", 
        "*.ac3", "*.adts", "*.mp2", "*.mpa", 
        "*.m4a", "*.mp4", "*.ogg", "*.aac", 
        "*.adt", "*.dts", "*.ape", "*.wv", 
        "*.caf", "*.gsm", "*.alaw", "*.ulaw", 
        "*.dwd", "*.snd", "*.au", "*.raw", 
        "*.pcm"
    ];
    public PlaylistCreateWindow()
    {
        InitializeComponent();
        Logger.Info("PlaylistCreateWindow opened");
        _playlistNameTextBox = this.FindControl<TextBox>("PlaylistNameTextBlock")!;
        
        _playlistBox = this.FindControl<ListBox>("PlaylistListBox")!;
        _songRemoveButton = this.FindControl<Button>("RemoveButton")!;
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e) => _playlistBox.Items.Remove(_playlistBox.SelectedItem);
    

    private void CreatePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Debug($"Songs: {_songs.Count} + {_songs.ToArray()} \n Info: {_playlistNameTextBox.Text!}");
            var newPlaylist = new Playlist
            {
                Name = _playlistNameTextBox.Text ?? "New Playlist",
                Songs = _songs!,
            };
        }
        catch (Exception ex)
        {
            Logger.Error($"Error with creating playlist: {ex}");
            throw;
        }
    }

    private async void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = GetTopLevel(this);
            var files = await topLevel?.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Audio File",
                AllowMultiple = true,
                FileTypeFilter = [new FilePickerFileType("Audio Files") { Patterns = _supportedAudioFormats }]
            })!;

            foreach (var file in files)
            {
                var name = file.Name;
                _playlistBox.Items.Add(name);
                _songs.Add(new SongData(name, file.Path.ToString(), TimeSpan.Zero));
            }

            _songRemoveButton.IsEnabled = true;
        }
        catch (Exception exception)
        {
           Logger.Error("Failed to open audio file" + exception); 
        }
    }
}