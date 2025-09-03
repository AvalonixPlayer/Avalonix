using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Avalonix.Models.Disk;

public static class DiskLoader
{
    public static async Task<T?> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
            return default;

        var opt = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        try
        {
            return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(path), opt)!;
        }
        catch (Exception)
        {
            return default;
        }
        
    }
}