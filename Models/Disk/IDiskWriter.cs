using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalonix.Models.Disk;

public interface IDiskWriter
{
    public async Task WriteAsync<T>(T obj, string path)
    {
        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        if (!File.Exists(path))
            File.Create(path).Close();
        
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(obj, opt));
    }
}