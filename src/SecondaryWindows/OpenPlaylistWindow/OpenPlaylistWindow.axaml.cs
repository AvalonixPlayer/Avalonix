using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonix.AvalonixAPI;
using static Avalonix.Program;

namespace Avalonix.SecondaryWindows;

public partial class OpenPlaylistWindow : Window
{
    private ListBox _playlistListBox;
    
    public OpenPlaylistWindow()
    {
        InitializeComponent();
        Logger.Info("PlaylistChangeWindow opened");
        _playlistListBox = this.FindControl<ListBox>("PlaylistsListBox")!;
        foreach (var playlist in Playlist.GetAllPlaylistNames())
        {
            Logger.Debug(playlist!);        
        }
    }
        
}