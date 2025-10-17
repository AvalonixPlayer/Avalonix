using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;

namespace Avalonix.ViewModels;

public interface IPlaylistSelectWindowViewModel
{
    Task<List<Playlist>> GetPlaylists();
    List<Playlist> SearchPlaylists(string text, List<Playlist> playlists);
    Task PlayPlaylist(Playlist playlist);
}