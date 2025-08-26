using System;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.UserSettings;
using Avalonix.Models.Media;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk;

public class DiskManager(ILogger logger) : IDiskManager 
{
    private readonly string _extension = ".avalonix";

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
            var path = Path.Combine(AvalonixFolderPath, ".settings" + _extension);
            if (!Path.Exists(path))
                File.Create(path).Close();
            return path;
        }
    }

    public string[] PlaylistsPaths => Directory.GetFiles(PlaylistsPath);

    public async Task SavePlaylist(Playlist playlist)
    {
        await ((IDiskWriter)this).WriteAsync(playlist, Path.Combine(PlaylistsPath, playlist.Name + _extension));
        logger.LogDebug("Playlist saved");
    }

    public async Task<Playlist> GetPlaylist(string name) =>
        (await ((IDiskLoader)this).LoadAsync<Playlist>(Path.Combine(PlaylistsPath, name + _extension)))!;

    public async Task SaveSettings(Settings settings)
    {
        await ((IDiskWriter)this).WriteAsync(settings, SettingsPath);
        logger.LogInformation("Settings saved");
    }

    public async Task<Settings?> GetSettings()
    {
        var result = await ((IDiskLoader)this).LoadAsync<Settings>(SettingsPath);
        if (result != null) return result;
        await SaveSettings(new Settings());
        result = await ((IDiskLoader)this).LoadAsync<Settings>(SettingsPath);
        return result;
    }
}

