using System.Threading.Tasks;

namespace Avalonix.Models.Disk;

public interface IDiskWriter
{
    Task WriteAsync<T>(T obj, string path);
}