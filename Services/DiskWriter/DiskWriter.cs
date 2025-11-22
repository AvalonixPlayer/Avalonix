using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonix.Services.DatabaseService;
using Avalonix.Services.Media.Playlist;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.DiskWriter;

public class DiskWriter(ILogger logger, IDatabaseService databaseService) : IDiskWriter
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
        catch (Exception ex)
        {
            logger.LogError("Error while writing json: {ex}", ex.Message);
        }
    }

    public async Task WritePlaylistToDb(Playlist playlist)
    {
        logger.LogInformation("Trying to Write Playlist: {plName}", playlist.Name);
        try
        {
            await databaseService.WritePlaylist(playlist);
        }
        catch (Exception ex)
        {
            logger.LogError("Error while writing playlist to db: {ex}", ex);
        }
    }
}
