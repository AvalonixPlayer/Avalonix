using System.Threading.Tasks;

namespace Avalonix.Services.DiskLoader;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
}
