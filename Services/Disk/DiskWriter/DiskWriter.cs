using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.Disk.DiskWriter;

public class DiskWriter(ILogger logger) : IDiskWriter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task WriteJsonAsync<T>(T obj, string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

        try
        {
            var json = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(obj, _jsonSerializerOptions));
        }
        catch (Exception e)
        {
            logger.LogError("Error while writing json: " + e.Message);
        }
    }
}
