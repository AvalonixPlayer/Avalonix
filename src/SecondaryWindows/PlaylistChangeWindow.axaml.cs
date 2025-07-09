using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonix.AvalonixAPI;
using static Avalonix.Program;

namespace Avalonix.SecondaryWindows;

public partial class PlaylistChangeWindow : Window
{
    private readonly TextBox _playlistNameTextBox;
    private readonly Button _songRemoveButton;
    private readonly Button _createPlaylistButton;
    private readonly ListBox _playlistBox;
    private readonly List<Song> _songs = [];
    
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
    
    public PlaylistChangeWindow()
    {
        InitializeComponent();
        Logger.Info("PlaylistCreateWindow opened");
        _playlistNameTextBox = this.FindControl<TextBox>("PlaylistNameTextBlock")!;
        _playlistBox = this.FindControl<ListBox>("PlaylistListBox")!;
        _songRemoveButton = this.FindControl<Button>("RemoveButton")!;
        _createPlaylistButton = this.FindControl<Button>("CreatePlaylistButton")!;
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _playlistBox.Items.Remove(_playlistBox.SelectedItem);
        if (_playlistBox.Items.Count == 0) _createPlaylistButton.IsEnabled = false;
    }

    private void CreatePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var newPlaylist = new Playlist
            {
                Name = _playlistNameTextBox.Text ?? "New_playlist",
                Songs = _songs
            };
            newPlaylist.SaveToJsonFile();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error with creating playlist: {ex}");
        }
        Close();
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
                _songs.Add(new Song(name, file.Path.ToString()));
            }

            _songRemoveButton.IsEnabled = true;
            _createPlaylistButton.IsEnabled = true; 
        }
        catch (Exception ex)
        {
           Logger.Error($"Failed to open audio file: {ex}"); 
        }
    }
}