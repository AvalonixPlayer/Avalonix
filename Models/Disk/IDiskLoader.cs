using System.Threading.Tasks;
using Avalonix.Models;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
    Task<T?> LoadAsyncWhithDependensies<T>(string path, object[] dependencies) where T : ILoadWithDependency;
}