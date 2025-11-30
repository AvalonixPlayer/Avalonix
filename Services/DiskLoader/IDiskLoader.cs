using System.Threading.Tasks;

namespace Avalonix.Services.DiskLoader;

public interface IDiskLoader
{
    Task<T?> LoadAsyncFromJson<T>(string path);
}