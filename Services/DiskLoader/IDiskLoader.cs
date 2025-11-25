using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;

namespace Avalonix.Services.DiskLoader;

public interface IDiskLoader
{
    Task<T?> LoadAsyncFromJson<T>(string path);
    Task<List<PlaylistData>> LoadAllPlaylistsFromDb();
}