using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media.Playlist;
using Avalonix.Model.Media.Track;
using Avalonix.Services.UserSettings.Theme;

namespace Avalonix.Services.DiskManager;

public interface IDiskManager
{
    Task SavePlaylist(Playlist playlist);
    Task RemovePlaylist(string name);
    Task SaveTracksMetadataCacheAsync(List<KeyValuePair<string, TrackMetadata>> pairs);
    Task<List<KeyValuePair<string, TrackMetadata>>?> LoadTracksMetadataCacheAsync();
    Task<List<PlaylistData>> GetAllPlaylists();
    Task CreateNewTheme(string name);
    Task SaveTheme(Theme theme);
    Task<Theme?> GetTheme(string name);
    List<string> GetMusicFiles(string path);
}