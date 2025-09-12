using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(IDiskManager idiskManager) : ViewModelBase
{
    public async Task<List<Playlist>> GetPlaylists() => await idiskManager.GetAllPlaylists();

    public static void SearchPlaylists(string text, List<Playlist> playlists, ref ListBox playlistBox)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            playlistBox.ItemsSource = playlists.Select(p => p.Name);
        }
            
        playlistBox.ItemsSource = playlists
            .Where(item => item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase))
            .Select(item => item.Name).ToList();
    }
}