using System.Threading.Tasks;

namespace Avalonix.Models.Disk.DiskLoader;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
}