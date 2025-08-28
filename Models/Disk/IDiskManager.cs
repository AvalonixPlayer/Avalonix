using System;
using System.IO;
using System.Threading.Tasks;
using Avalonix.Models.Media.MediaPlayerFiles;
using Avalonix.Models.UserSettings;
using Avalonix.Models.Media.PlaylistFiles;

namespace Avalonix.Models.Disk;

public interface IDiskManager : IDiskWriter, IDiskLoader
{
    Task SavePlaylist(Playlist playlist);
    Task<Playlist> GetPlaylist(string name, IMediaPlayer player, IDiskManager diskManager);
    
    Task SaveSettings(Settings settings);
    Task<Settings> GetSettings();
    
    string _extension => ".avalonix";
    
    string AvalonixFolderPath
    {
        get
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".avalonix");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    
    string PlaylistsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".playlists");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
    
    string SettingsPath
    {
        get
        {
            var path = Path.Combine(AvalonixFolderPath, ".settings" + _extension);
            if (!Path.Exists(path))
                File.Create(path).Close();
            return path;
        }
    }
}