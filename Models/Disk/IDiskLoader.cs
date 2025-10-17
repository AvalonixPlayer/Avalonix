using System.Threading.Tasks;

namespace Avalonix.Models.Disk;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
}