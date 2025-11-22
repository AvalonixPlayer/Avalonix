using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;

namespace Avalonix.Services.DiskWriter;

public interface IDiskWriter
{
    Task WriteJsonAsync<T>(T obj, string path);
    Task WritePlaylistToDb(Playlist playlist);
}
