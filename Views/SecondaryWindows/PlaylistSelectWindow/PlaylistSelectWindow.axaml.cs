using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModels.PlaylistSelect;

namespace Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

public partial class PlaylistSelectWindow : Window
{
    private readonly ILogger<WindowManager> _logger;
    private readonly IPlaylistSelectWindowViewModel _vm;
    private readonly List<Playlist> _playlists;
    public PlaylistSelectWindow(ILogger<WindowManager> logger, IPlaylistSelectWindowViewModel vm)
    {
        InitializeComponent();
        _logger = logger;
        _vm = vm;
        _logger.LogInformation("PlaylistCreateWindow opened");
        
        _playlists = Task.Run(async () => await _vm.GetPlaylists()).Result;
        
        var result = _playlists.Select(p => p.Name).ToList();
        PlaylistBox.ItemsSource = result;
    }


    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            PlaylistBox.ItemsSource = _playlists.Select(p => p.Name);
            return;
        }
        var stringPlaylists = _vm.SearchPlaylists(text, _playlists).Select(p => p.Name).ToList();
        PlaylistBox.ItemsSource = stringPlaylists;
    }

    private async void StartSelectedPlaylist(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            var castedSender = (ListBox)sender!;
            _logger.LogInformation(castedSender.SelectedItem?.ToString());
            var selectedPlaylist = _playlists.FirstOrDefault(p => p.Name == castedSender.SelectedItem?.ToString());
            await _vm.PlayPlaylist(selectedPlaylist!);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while starting playlist in SelectWindow: {ex}", ex.Message);
        }
    }
}