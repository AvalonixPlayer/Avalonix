using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.DiskLoader;

public class DiskLoader(ILogger logger) : IDiskLoader
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task<T?> LoadAsyncFromJson<T>(string path)
    {
        if (!File.Exists(path))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path), _jsonSerializerOptions) ?? default;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to load json: {ex}", ex.Message);
            return default;
        }
    }
}