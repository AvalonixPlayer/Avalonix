using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk;

public class DiskWriter(ILogger logger) : IDiskWriter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    
    public async Task WriteAsync<T>(T obj, string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

        try
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(obj, _jsonSerializerOptions));
        }
        catch (Exception e)
        {
            logger.LogError("Error while writing json: " + e.Message);
        }
    }
}