using System.Threading.Tasks;

namespace Avalonix.Services.DiskWriter;

public interface IDiskWriter
{
    Task WriteJsonAsync<T>(T obj, string path);
}
