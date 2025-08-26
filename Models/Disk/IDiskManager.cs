using System.Threading.Tasks;
using Avalonix.Models.UserSettings;
using Avalonix.Models.Media;

namespace Avalonix.Models.Disk;

public interface IDiskManager : IDiskWriter, IDiskLoader
{
    Task SavePlaylist(Playlist playlist);
    Task<Playlist> GetPlaylist(string name);
    
    Task SaveSettings(Settings settings);
    Task<Settings?> GetSettings();
    
    string[] PlaylistsPaths { get; }
}