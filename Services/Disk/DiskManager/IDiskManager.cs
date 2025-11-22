using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Services.Media.Playlist;
using Avalonix.Services.UserSettings.Theme;

namespace Avalonix.Services.Disk.DiskManager;

public interface IDiskManager
{
    Task SavePlaylist(Playlist playlist);
    Task<Playlist?> GetPlaylist(string name);
    void RemovePlaylist(string name);
    Task<List<Playlist>> GetAllPlaylists();
    Task CreateNewTheme(string name);
    Task SaveTheme(Theme theme);
    Task<Theme?> GetTheme(string name);
    List<string> GetMusicFilesForAlbums();
}