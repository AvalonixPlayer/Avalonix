using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.Playlist;
using Avalonix.Models.Media.Track;

namespace Avalonix.Services.PlaylistManager;

public interface IPlaylistManager
{
    Playlist ConstructPlaylist(string title, List<Track> tracks);
    Task EditPlaylist(Playlist playlist);
    Task CreatePlaylist(Playlist playlist);
    Task StartPlaylist(Playlist playlist);
}