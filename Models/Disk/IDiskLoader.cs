using System.Threading.Tasks;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
}