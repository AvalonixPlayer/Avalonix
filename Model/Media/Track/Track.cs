using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Track;

[NotMapped]
public class Track
{
    [JsonIgnore] public TrackMetadata Metadata = new();

    public Track()
    {
    }

    public Track(string path)
    {
        TrackData = new TrackData(path);
    }

    public TrackData TrackData { get; } = null!;


    public void IncreaseRarity(int rarity)
    {
        TrackData.Rarity += rarity;
    }

    public void UpdateLastListenDate()
    {
        TrackData.LastListen = DateTime.Now.TimeOfDay;
    }
}