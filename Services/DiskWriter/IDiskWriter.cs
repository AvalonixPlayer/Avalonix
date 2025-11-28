using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;

namespace Avalonix.Services.DiskWriter;

public interface IDiskWriter
{
    Task WriteJsonAsync<T>(T obj, string path);
}