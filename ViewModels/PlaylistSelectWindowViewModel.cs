using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Avalonix.Models.Media.Playlist;
using Avalonix.Services.PlaylistManager;

namespace Avalonix.ViewModels;

public class PlaylistSelectWindowViewModel(IDiskManager diskManager, IPlaylistManager playlistManager) : ViewModelBase, IPlaylistSelectWindowViewModel
{
    public async Task<List<Playlist>> GetPlaylists() => await diskManager.GetAllPlaylists();
    public List<Playlist> SearchPlaylists(string text, List<Playlist> playlists) =>
        string.IsNullOrWhiteSpace(text) ? playlists : playlists.Where(item => item.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToList();

    public async Task PlayPlaylist(Playlist playlist)
    {
        // and change main window
        await playlistManager.StartPlaylist(playlist);
    }
}