using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using static Avalonix.Program;

namespace Avalonix.SecondaryWindows;

public partial class PlaylistManagerWindow : Window
{
    public PlaylistManagerWindow()
    {
        InitializeComponent();
    }

    private async void AddPlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new PlaylistCreateWindow();
            await dialog.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error adding playlist: {ex}");
        }
    }

    private void RemovePlaylistButton_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}