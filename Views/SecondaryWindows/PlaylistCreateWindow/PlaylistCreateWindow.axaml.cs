using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.ViewModels;
using Microsoft.Extensions.Logging;

namespace Avalonix.Views.SecondaryWindows.PlaylistCreateWindow;

public partial class PlaylistCreateWindow : Window
{
    private readonly PlaylistCreateWindowViewModel _vm;
    private readonly ILogger _logger;
    public PlaylistCreateWindow(ILogger logger, PlaylistCreateWindowViewModel vm)
    {
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        logger.LogInformation("PlaylistCreateWindow opened");
    }

    private async void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var list = await _vm.OpenTrackFileDialog(this);
            NewSongBox.Items.Add(list);
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
            var songs2remove = NewSongBox.SelectedItems!; 
            NewSongBox.Items.Remove(songs2remove);
            _logger.LogInformation("Removed songs: {songs}", songs2remove );
            
            if (NewSongBox.Items.Count == 0) 
                RemoveButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when remove songs: {ex}", ex.Message);
        }
    }
}