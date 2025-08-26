using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix.Models.Disk;

public interface IDiskLoader
{
    public async Task<T?> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path), opt);
    }
}