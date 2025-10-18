using System;
using System.Text.Json.Serialization;

namespace Avalonix.Models.Media.Track;

public class Track
{
    public TrackData TrackData;
    [JsonIgnore] public TrackMetadata Metadata => new TrackMetadata(TrackData.Path);

    [JsonConstructor]
    public Track()
    {
    }

    public Track(string path)
    {
        TrackData = new (path);
    }

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}