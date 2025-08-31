using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

public partial class PlaylistSelectWindow : Window
{
    private readonly ILogger _logger;
    private readonly PlaylistSelectWindowViewModel _vm;
    private readonly List<Playlist> _playlists;
    public PlaylistSelectWindow(ILogger logger, PlaylistSelectWindowViewModel vm)
    {
        InitializeComponent();
        _logger = logger;
        _vm = vm;
        _logger.LogInformation("PlaylistCreateWindow opened");
        
        _playlists = Task.Run(async () => await _vm.GetPlaylists()).GetAwaiter().GetResult();
        _playlists.ForEach(item => _logger.LogDebug(item.Name));
        var result = _playlists.Select(p => p.Name).ToList();
        PlaylistBox.ItemsSource = result;
    }

    private void StartPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selectedPlaylist = PlaylistBox.SelectedItems!;
            if (selectedPlaylist.Count == 0)
                return;
            var playlist = selectedPlaylist[0]!; 
        }
        catch (Exception exception)
        {
            _logger.LogError("Error while starting playlist in SelectWindow: {ex}", exception.Message);
        }
    }

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text;
        if (string.IsNullOrWhiteSpace(text))
            PlaylistBox.ItemsSource = _playlists.Select(p => p.Name).ToList();

        PlaylistBox.ItemsSource = _playlists
            .Where(item => item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase))
            .Select(item => item.Name);
    }
}