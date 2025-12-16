using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.Media.Track;
using Avalonix.Model.UserSettings.Theme;

namespace Avalonix.Services.DiskManager;

public interface IDiskManager
{
    Task SavePlaylist(Playlist playlist);
    Task RemovePlaylist(string name);
    Task<List<PlaylistData>> GetAllPlaylists();
    Task CreateNewTheme(string name);
    Task SaveTheme(Theme theme);
    Task<Theme?> GetTheme(string name);
    List<string> GetMusicFiles();
}
