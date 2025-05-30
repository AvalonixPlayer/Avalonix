using System;
using System.IO;

namespace AvalonixAPI;

public static class DiskManager
{
    public static string EnvPath() => Path.GetDirectoryName(Environment.ProcessPath) ?? "";
    
    public static void EnsurePlaylistsDirectoryExists()
    {
        if (!Directory.Exists(Settings.PlaylistsDirectory))
        {
            Directory.CreateDirectory(Settings.PlaylistsDirectory);
        }
    }
    
    public static string GetFullPath(string relativePath)
    {
        return Path.IsPathRooted(relativePath) 
            ? relativePath 
            : Path.Combine(EnvPath(), relativePath);
    }
}
