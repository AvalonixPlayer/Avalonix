using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;

namespace Avalonix.Services.PlayableManager.PlaylistManager;

public interface IPlaylistManager : IPlayableManager
{
    Playlist ConstructPlaylist(string title, List<string> tracks, string? observingDirectory);
    Task EditPlaylist(Playlist playlist);
    Task CreatePlaylist(Playlist playlist);
    void DeletePlaylist(Playlist playlist);
    Task<List<PlaylistData>> GetAllPlaylistData();
}