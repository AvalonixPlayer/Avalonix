using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;

namespace Avalonix.Services.DiskLoader;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
    Task<List<Playlist>> LoadAllPlaylistsFromDb();
}
