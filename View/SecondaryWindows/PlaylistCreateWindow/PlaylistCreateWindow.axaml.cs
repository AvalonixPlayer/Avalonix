using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonix.Model.Media.Track;
using Avalonix.Services.WindowManager;
using Avalonix.ViewModels.PlaylistEditOrCreate;
using Microsoft.Extensions.Logging;

namespace Avalonix.View.SecondaryWindows.PlaylistCreateWindow;

public partial class PlaylistCreateWindow : Window
{
    private readonly IPlaylistEditOrCreateWindowViewModel _vm;
    private readonly ILogger<WindowManager> _logger;

    private string? _observingDirectory;

    public PlaylistCreateWindow(ILogger<WindowManager> logger, IPlaylistEditOrCreateWindowViewModel vm)
    {
        _logger = logger;
        _vm = vm;
        InitializeComponent();
        _logger.LogInformation("PlaylistCreateWindow opened");
        MainActionButton.Content = _vm.Strategy.ActionButtonText;
        NameLabel.Content = _vm.Strategy.WindowTitle;
        Title += _vm.Strategy.WindowTitle;
        ObserveDirectory.IsCheckedChanged += ObserveDirectoryCheckedChanged;
    }

    private void ObserveDirectoryCheckedChanged(object? sender, RoutedEventArgs routedEventArgs)
    {
        var isChecked = ((CheckBox)routedEventArgs.Source!).IsChecked;
        if (isChecked != null)
            ObservingDirectory.IsEnabled = isChecked.Value;
    }

    private async void AddButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var fileList = (await _vm.OpenTrackFileDialogAsync(this))!;

            if (fileList.Any(string.IsNullOrWhiteSpace)) return;

            fileList.ToList().ForEach(item => NewSongBox.Items.Add(item));

            RemoveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when adding songs: {ex}", ex.Message);
        }
    }

    private async void ObservingDirectory_OnClick(object? sender, RoutedEventArgs e)
    {
        var observingDirectory = await _vm.OpenObservingDirectoryDialogAsync(this);
        if(string.IsNullOrEmpty(observingDirectory)) return;
        _observingDirectory = observingDirectory;
    }

    private void RemoveButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var songs2Remove = NewSongBox.SelectedIndex;
            NewSongBox.Items.RemoveAt(songs2Remove);
            _logger.LogInformation("Removed songs: {songs}", songs2Remove );

            if (NewSongBox.Items.Count.Equals(0))
                RemoveButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when remove songs: {ex}", ex.Message);
        }
    }

    private async void ActionPlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var name = PlaylistName.Text;
            if(string.IsNullOrEmpty(name)) return;
            var items = NewSongBox.Items.OfType<string>().ToList();
            List<Track> tracks = [];
            items.ForEach(item => tracks.Add(new Track(item)));
            await _vm.ExecuteAsync(name, tracks, _observingDirectory);
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error when create playlist: {ex}", ex);
        }
    }
}
