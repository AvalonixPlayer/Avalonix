using System.Threading.Tasks;

public interface IDiskWriter
{
    Task WriteAsync<T>(T obj, string path);
}