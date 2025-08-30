using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.UserSettings;
using Avalonix.Models.Media.PlaylistFiles;
using Microsoft.Extensions.Logging;

namespace Avalonix.Models.Disk;

public class DiskManager(ILogger logger, IMediaPlayer player) : IDiskManager
{
    private IDiskManager IDM => this;

    public async Task SavePlaylist(Playlist playlist)
    {
        await IDM.WriteAsync(playlist, Path.Combine(IDM.PlaylistsPath, playlist.Name + IDM._extension));
        logger.LogDebug("Playlist saved");
    }

    public async Task<Playlist> GetPlaylist(string name)
    {
        var result = (await IDM.LoadAsync<Playlist>(Path.Combine(IDM.PlaylistsPath, name + IDM._extension)))!;
        result.Initialize(name, player, IDM,logger);
        return result;
    }


    public async Task SaveSettings(Settings settings)
    {
        await IDM.WriteAsync(settings, IDM.SettingsPath);
        logger.LogInformation("Settings saved");
    }

    public async Task<Settings> GetSettings()
    {
        var result = await IDM.LoadAsync<Settings>(IDM.SettingsPath);
        if (result != null) return result;
        await SaveSettings(new Settings());
        result = await IDM.LoadAsync<Settings>(IDM.SettingsPath);
        return result;
    }
}