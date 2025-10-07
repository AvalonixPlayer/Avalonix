using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Services;

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
        
        _playlists = Task.Run(async () => await _vm.GetPlaylists()).GetAwaiter().GetResult();
        
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
            
        PlaylistBox.ItemsSource = _playlists
            .Where(item => item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase))
            .Select(item => item.Name);
    }

    private void StartSelectedPlaylist(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            /*
            var selectedPlaylist = (List<string>)PlaylistBox.SelectedItems!;
            if (selectedPlaylist.Count == 0)
                return;
            var playlist = selectedPlaylist[0];
            */
            _ = _playlists[0].Play();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while starting playlist in SelectWindow: {ex}", ex.Message);
        }
    }
}