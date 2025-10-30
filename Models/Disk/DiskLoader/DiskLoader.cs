using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk.DiskLoader;

public class DiskLoader(ILogger logger) : IDiskLoader
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<T?> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path), _jsonSerializerOptions)!;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to load json: " + ex.Message);
            return default;
        }
    }
}