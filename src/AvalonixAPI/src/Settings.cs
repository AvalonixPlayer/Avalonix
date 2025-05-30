using System.IO;
using System;

namespace AvalonixAPI;

public static class Settings
{
    public static bool LoopingPlaylists { get; set; } = true;
    public static bool Shuffle { get; set; }

    public static string PlaylistsDirectory
    {
        get
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, ".avalonix");
        }
    }
}