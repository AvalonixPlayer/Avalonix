using System.Threading.Tasks;
using Avalonix.Models;

public interface IDiskLoader
{
    Task<T?> LoadAsync<T>(string path);
}