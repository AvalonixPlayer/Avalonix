using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.Media.Track;

namespace Avalonix.Services.PlayableManager.PlaylistManager;

public interface IPlaylistManager : IPlayableManager
{
    Playlist ConstructPlaylist(string title, List<Track> tracks, string? observingDirectory);
    Task EditPlaylist(Playlist playlist);
    Task CreatePlaylist(Playlist playlist);
    void DeletePlaylist(Playlist playlist);
    Task<List<PlaylistData>> GetAllPlaylistData();
}