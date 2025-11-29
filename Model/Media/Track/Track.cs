using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Track;

public class Track
{
    [JsonIgnore] public TrackMetadata Metadata = new();
    [JsonInclude] public TrackData TrackData;

    [JsonConstructor]
    public Track()
    {
    }

    public Track(string path)
    {
        TrackData = new TrackData(path);
    }

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}