using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NeoSimpleLogger;

namespace Avalonix.Models.Disk;

public interface IDiskLoader
{
    public T? Load<T>(string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), opt);
        }
        catch (JsonException e)
        {
            new Logger().LogError(e.Message);
        }

        return default;
    }
}