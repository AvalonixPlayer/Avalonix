using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;

namespace Avalonix.Services.DatabaseService;

public interface IDatabaseService
{
    Task WritePlaylistData(PlaylistData playlist);
    void RemovePlaylistData(PlaylistData playlist);
    void RemovePlaylistData(string plName);
    Task<List<PlaylistData>> GetAllPlaylists();
}

