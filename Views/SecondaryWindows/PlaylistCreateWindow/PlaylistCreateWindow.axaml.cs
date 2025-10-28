using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Models.Media.Track;
using Avalonix.Services;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;

public partial class PlaylistCreateWindow : Window
{
    private readonly IPlaylistEditOrCreateWindowViewModel _vm;
    private readonly ILogger<WindowManager> _logger;
        
    public PlaylistCreateWindow(ILogger<WindowManager> logger, IPlaylistEditOrCreateWindowViewModel vm)
    {
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        _logger.LogInformation("PlaylistCreateWindow opened");
    }

    private async void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var fileList = (await _vm.OpenTrackFileDialogAsync(this).ConfigureAwait(false))!;
            
            if (fileList.Any(string.IsNullOrWhiteSpace)) return;
            
            fileList.ToList().ForEach(item => NewSongBox.Items.Add(item));
            
            RemoveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when adding songs: {ex}", ex.Message);
        }
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var songs2Remove = NewSongBox.SelectedItems!; 
            NewSongBox.Items.Remove(songs2Remove);
            _logger.LogInformation("Removed songs: {songs}", songs2Remove );
            
            if (NewSongBox.Items.Count.Equals(0)) 
                RemoveButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when remove songs: {ex}", ex.Message);
        }
    }

    private async void CreatePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var name = PlaylistName.Text;
            var items = NewSongBox.Items.OfType<string>().ToList();
            if (string.IsNullOrWhiteSpace(name) || items.Count <= 0) return;

            List<Track> tracks = [];
            
            items.ForEach(item => tracks.Add(new Track(item)));
            await _vm.ExecuteAsync(name, tracks);
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when create playlist: {ex}", ex);
        }
    }
}