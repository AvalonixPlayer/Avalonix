using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media;
using Avalonix.Services.Media.Playlist;
using Avalonix.Services.Media.Track;

namespace Avalonix.Services.PlayableManager.PlaylistManager;

public interface IPlaylistManager : IPlayableManager
{
    Playlist ConstructPlaylist(string title, List<Track> tracks, string? observingDirectory);
    Task EditPlaylist(Playlist playlist);
    Task CreatePlaylist(Playlist playlist);
    void DeletePlaylist(Playlist playlist);
    Task StartPlaylist(IPlayable playlist);
    Task<List<Playlist>> GetAllPlaylists();
}
