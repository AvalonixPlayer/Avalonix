using System.Threading.Tasks;

namespace Avalonix.Services.Disk.DiskLoader;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
}