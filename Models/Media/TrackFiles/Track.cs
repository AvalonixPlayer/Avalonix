using System;
using System.Text.Json.Serialization;

namespace Avalonix.Models.Media.TrackFiles;

public class Track
{
    public TrackData TrackData;
    [JsonIgnore] public TrackMetadata Metadata;

    [JsonConstructor]
    public Track()
    {
    }

    public Track(string? path)
    {
        TrackData = new TrackData(path!);
        Metadata = new(TrackData.Path);
    }

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}