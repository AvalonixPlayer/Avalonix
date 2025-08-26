using System;
using System.IO;
using System.Threading.Tasks;
using Avalonix.API;
using Avalonix.Models.Media;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk;

public class DiskManager(ILogger logger) : IDiskWriter, IDiskLoader
{
    private readonly string Extension = ".avalonix";

    private string AvalonixFolderPath
    {
        get
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    private string PlaylistsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".playlists");
            if (!Path.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    private string SettingsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".settings" + Extension);
            if (!Path.Exists(path))
                File.Create(path).Close();
            return path;
        }
    }

    private string[] PlaylistsPaths => Directory.GetFiles(PlaylistsPath);


    public async Task SavePlaylist(Playlist playlist)
    {
        await ((IDiskWriter)this).WriteAsync(playlist, Path.Combine(PlaylistsPath, playlist.Name + Extension));
        logger.LogInformation("Playlist saved");
    }

    public async Task<Playlist> GetPlaylist(string name)
    {
        var result = await ((IDiskLoader)this).LoadAsync<Playlist>(Path.Combine(PlaylistsPath, name + Extension));
        if (result == null)
            SavePlaylist(new Playlist(name));
        return result; 
    }

    public async Task SaveSettings(Settings settings)
    {
        await ((IDiskWriter)this).WriteAsync(settings, SettingsPath);
        logger.LogInformation("Settings saved");
    }

    public async Task<Settings?> GetSettings()
    {
        var result = await ((IDiskLoader)this).LoadAsync<Settings>(SettingsPath);
        if (result == null)
            SaveSettings(new Settings());
           result = await ((IDiskLoader)this).LoadAsync<Settings>(SettingsPath);
        return result;
    }
}

