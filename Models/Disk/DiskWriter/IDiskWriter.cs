using System.Threading.Tasks;

namespace Avalonix.Models.Disk.DiskWriter;

public interface IDiskWriter
{
    Task WriteJsonAsync<T>(T obj, string path);
}