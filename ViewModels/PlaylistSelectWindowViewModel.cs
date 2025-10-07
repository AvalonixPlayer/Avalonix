using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(IDiskManager idiskManager) 
    : ViewModelBase, IPlaylistSelectWindowViewModel
{
    public async Task<List<Playlist>> GetPlaylists() => await idiskManager.GetAllPlaylists();
    public List<Playlist> SearchPlaylists(string text, List<Playlist> playlists) =>
        string.IsNullOrWhiteSpace(text) ? playlists : playlists.Where(item => item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task PlayPlaylist(Playlist playlist)
    {
        // and change main window
        await playlist.Play();
    }
}