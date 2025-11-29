using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.DiskWriter;

public class DiskWriter(ILogger logger) : IDiskWriter
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public async Task WriteJsonAsync<T>(T obj, string path)
    {
        if (!File.Exists(path))
            File.Create(path).Close();

        try
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(obj, _jsonSerializerOptions));
        }
        catch (Exception ex)
        {
            logger.LogError("Error while writing json: {ex}", ex.Message);
        }
    }
}