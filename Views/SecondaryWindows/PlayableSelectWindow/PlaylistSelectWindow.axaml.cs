using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModels.PlayableSelectViewModel;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlayableSelectWindow;

public partial class PlayableSelectWindow : Window
{
    private readonly ILogger<WindowManager> _logger;
    private readonly List<Playlist> _playlists;
    public PlayableSelectWindow(ILogger<WindowManager> logger, IPlayableSelectViewModel vm)
    {
        InitializeComponent();
        _logger = logger;
        _vm = vm;
        _logger.LogInformation("PlaylistCreateWindow opened");

        _playlists = Task.Run(async () => await _vm.GetPlaylists()).Result;

        var result = _playlists.Select(p => p.Name).ToList();
        PlaylistBox.ItemsSource = result;
        Title += _vm.Strategy.WindowTitle;
        SearchBox.Watermark = _vm.Strategy.ActionButtonText;
    }


    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            PlaylistBox.ItemsSource = _playlists.Select(p => p.Name);
            return;
        }
        var stringPlaylists = _vm.SearchItem(text, _playlists).Select(p => p.Name).ToList();
        PlaylistBox.ItemsSource = stringPlaylists;
    }

    private async void ActionSelectedPlaylist(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            var castedSender = (ListBox)sender!;
            _logger.LogInformation(castedSender.SelectedItem?.ToString());
            var selectedPlaylist = _playlists.FirstOrDefault(p => p.Name == castedSender.SelectedItem?.ToString());
            await _vm.ExecuteAction(selectedPlaylist!);
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while starting playlist in SelectWindow: {ex}", ex.Message);
        }
    }
}
