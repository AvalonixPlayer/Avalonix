using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;

namespace Avalonix.Services.DatabaseService;

public interface IDatabaseService
{
    Task WritePlaylist(Playlist playlist);
    void RemovePlaylist(Playlist playlist);
    void RemovePlaylist(string plName, List<Playlist> playlists);
    Task<List<Playlist>> GetAllPlaylists();
}

