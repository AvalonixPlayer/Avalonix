using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AvalonixAPI;

public struct PlaylistData
{
    public string Name { get; set; }
    public int Year { get; set; }
    public int PlayCount { get; set; }
    public DateTime LastPlayed { get; set; }
    public List<SongInfo> Songs { get; set; }

    public struct SongInfo
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public TimeSpan Duration { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
    }

    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
    
    public static PlaylistData FromJson(string json) => 
        JsonConvert.DeserializeObject<PlaylistData>(json);
}