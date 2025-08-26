using System;
using System.IO;
using System.Text.Json;
using Avalonix.API;
using Avalonix.Models.Media;
using Microsoft.Extensions.Logging;
using Logger = NeoSimpleLogger.Logger;

namespace Avalonix.Models.Disk;

public class DiskManager : IDiskWriter, IDiskLoader
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


    public void SavePlaylist(Playlist playlist) =>
        ((IDiskWriter)this).Write(playlist, Path.Combine(PlaylistsPath, playlist.Name + Extension));

    public Playlist GetPlaylist(string name)
    {
        var result = ((IDiskLoader)this).Load<Playlist>(Path.Combine(PlaylistsPath, name + Extension));
        if (result == null)
            SavePlaylist(new Playlist(name));
        return ((IDiskLoader)this).Load<Playlist>(Path.Combine(PlaylistsPath, name + Extension));
    }

    public void SaveSettings(Settings settings) =>
        ((IDiskWriter)this).Write(settings, SettingsPath);

    public Settings GetSettings()
    {
        var result = ((IDiskLoader)this).Load<Settings>(SettingsPath);
        if (result == null)
            SaveSettings(new Settings());
        return ((IDiskLoader)this).Load<Settings>(SettingsPath);
    }
}

