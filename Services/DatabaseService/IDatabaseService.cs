using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;

namespace Avalonix.Services.DatabaseService;

public interface IDatabaseService
{
    Task WritePlaylist(Playlist playlist);
    Task RemovePlaylist(Playlist playlist);
    Task<List<Playlist>> GetAllPlaylists();
}

