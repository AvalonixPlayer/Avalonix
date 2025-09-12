using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Models.Media.PlaylistFiles;
using Avalonix.Models.UserSettings;
using Avalonix.Models.UserSettings.Theme;

namespace Avalonix.Models.Disk;

public interface IDiskManager
{
    Task SavePlaylist(Playlist playlist);
    Task<Playlist?> GetPlaylist(string name);
    void RemovePlaylist(string name);
    Task<List<Playlist>> GetAllPlaylists();
    Task SaveSettings(Settings settings);
    Task CreateNewTheme(string name);
    Task SaveTheme(Theme theme);
    Task<Theme?> GetTheme(string name);
    Task<Settings> GetSettings();
}