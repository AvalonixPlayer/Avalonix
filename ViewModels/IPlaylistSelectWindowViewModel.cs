using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.ViewModels;

public interface IPlaylistSelectWindowViewModel
{
    Task<List<Playlist>> GetPlaylists();
    List<Playlist> SearchPlaylists(string text, List<Playlist> playlists);
    Task PlayPlaylist(Playlist playlist);
}