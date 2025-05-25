using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            string pathToPlaylist = EnvPath() + "\\playlists\\" + playlistName + ".json";
            string json = File.Exists(pathToPlaylist) ? File.ReadAllText(pathToPlaylist) : null!;
            string[] jsonObj = JsonConvert.DeserializeObject<string[]>(json)!;
            return jsonObj;
        }

        public static string[] Playlists()
        {
            return Directory.GetFiles(EnvPath() + "\\playlists\\", "*.json");
        }

        public static void CreatePlaylist(string playlistName)
        {
            Directory.CreateDirectory(EnvPath() + "\\playlists\\" + playlistName + ".json");
        }

        public static void AddToPlaylist(string playlistName, string musicPath)
        {
            List<string> list = GetAudios(playlistName).ToList();
            list.Add(musicPath);
            string json = JsonConvert.SerializeObject(list);
            File.WriteAllText(EnvPath() + "\\playlists\\" + playlistName + ".json", json);
        }
    }
}