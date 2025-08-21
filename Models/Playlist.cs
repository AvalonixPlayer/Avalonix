using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Avalonix.API;

public class Playlist(string name)
{
    public string Name => name;
    [JsonInclude] public List<Track> Tracks { get; private set; } = [];

    public void AddTrack(Track track) => Tracks.Add(track);
}