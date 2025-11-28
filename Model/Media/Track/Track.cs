using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Track;

public class Track
{
    [JsonIgnore] public readonly TrackMetadata Metadata = new();
    public readonly TrackData TrackData;
    [Key] public string Path { get; init; }

    public Track()
    {
    }

    public Track(string path)
    {
        Path = path;
        TrackData = new TrackData(path);
    }

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}