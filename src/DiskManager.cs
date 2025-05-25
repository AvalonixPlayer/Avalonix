using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAPI
{
    public static class DiskManager
    {
        public static string[] GetAudios(string playlistName)
        {
            return Directory.GetFiles(Path.GetDirectoryName(Environment.ProcessPath) + "\\playlists\\" + playlistName);
        }

        public static string[] Playlists()
        {
            return Directory.GetDirectories(Path.GetDirectoryName(Environment.ProcessPath) + "\\playlists\\");
        }
    }
}