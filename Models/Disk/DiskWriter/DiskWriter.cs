using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonix.Models.Disk;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk.DiskWriter;

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
            var json = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(obj, _jsonSerializerOptions));
        }
        catch (Exception e)
        {
            logger.LogError("Error while writing json: " + e.Message);
        }
    }
}