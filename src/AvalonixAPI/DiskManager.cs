using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvalonixAPI
{
    public static class DiskManager
    {
        public static string EnvPath()
        {
            return Path.GetDirectoryName(Environment.ProcessPath) ?? "";
        }

        public static string[] GetAudios(string playlistName)
        {
            return Directory.GetFiles(EnvPath() + "\\playlists\\" + playlistName);
        }

        public static string[] Playlists()
        {
            return Directory.GetDirectories(EnvPath() + "\\playlists\\");
        }

        public static void CreatePlaylist(string playlistName)
        {
            Directory.CreateDirectory(EnvPath() + "\\playlists\\" + playlistName);
        }
    }
}