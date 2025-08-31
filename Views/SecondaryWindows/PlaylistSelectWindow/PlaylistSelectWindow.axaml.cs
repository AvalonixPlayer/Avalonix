using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

public partial class PlaylistSelectWindow : Window
{
    private readonly ILogger _logger;
    public PlaylistSelectWindow(ILogger logger, PlaylistSelectWindowViewModel vm)
    {
        InitializeComponent();
        _logger = logger;
        try
        {
            var playlists = vm.GetPlaylists();
            Playlists.ItemsSource = playlists;
        }
        catch (Exception exception)
        {
            _logger.LogError("Error with opening window: {ex}", exception.Message);
        }
        finally
        {
            _logger.LogInformation("PlaylistCreateWindow opened");
        }
    }

    private void StartPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var playlists = Playlists.SelectedItems;
            if (playlists!.Count == 0)
                return;
            var playlist = (Playlist)playlists[0]!; 
            _ = playlist.Play();
        }
        catch (Exception exception)
        {
            _logger.LogError("Error while starting playlist in SelectWindow: {ex}", exception.Message);
        }
    }
}