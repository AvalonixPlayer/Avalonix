using System.Threading.Tasks;

namespace Avalonix.Models.DiskWriter;

public interface IDiskWriter
{
    Task WriteAsync<T>(T obj, string path);
}