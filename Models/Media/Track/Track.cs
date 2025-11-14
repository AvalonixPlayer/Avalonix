using System;
using System.Text.Json.Serialization;

namespace Avalonix.Models.Media.Track;

public class Track
{
    [JsonInclude] public TrackData TrackData { get; set; } = null!;
    [JsonIgnore] public TrackMetadata Metadata = new();

    [JsonConstructor]
    public Track() { }

    public Track(string path) => TrackData = new TrackData(path);

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}