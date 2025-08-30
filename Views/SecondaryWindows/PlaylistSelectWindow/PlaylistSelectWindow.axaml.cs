using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlaylistSelectWindow;

public partial class PlaylistSelectWindow : Window
{
    private List<Playlist> _playlists;
    public PlaylistSelectWindow(ILogger logger, PlaylistSelectWindowViewModel vm)
    {
        InitializeComponent();
        logger.LogInformation("PlaylistCreateWindow opened");
    }

    private void StartPlaylist_OnClick(object? sender, RoutedEventArgs e)
    {
    }
}