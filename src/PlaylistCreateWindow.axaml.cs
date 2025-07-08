using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonix.AvalonixAPI;
using static Avalonix.Program;

namespace Avalonix;

public partial class PlaylistCreateWindow : Window
{
    private readonly TextBox _playlistNameTextBox;
    private readonly Dictionary<string, TextBox> _optionalMedataTextBoxes;
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
        _optionalMedataTextBoxes["Album"] = this.FindControl<TextBox>("AlbumTextBox")!;
        _optionalMedataTextBoxes["Performer"] = this.FindControl<TextBox>("PerformerTextBox")!;
        _optionalMedataTextBoxes["Year"] = this.FindControl<TextBox>("YearTextBox")!;
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e) => _playlistBox.Items.Remove(_playlistBox.SelectedItem);
    

    private void CreatePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {

            PlaylistsManager.CreateNewPlaylist(new PlaylistData
            {
                Name = _playlistNameTextBox.Text ?? "New Playlist",
                Album = _optionalMedataTextBoxes["Album"].Text,
                Performer = _optionalMedataTextBoxes["Performer"].Text,
                Songs = _songs,
                Year = Convert.ToInt32(_optionalMedataTextBoxes["Year"].Text), 
            });

            var newPlaylist = new Playlist
            {
                Name = _playlistNameTextBox.Text ?? "New Playlist",
                Songs = _songs,
            };
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
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