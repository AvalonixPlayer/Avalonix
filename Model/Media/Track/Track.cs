using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Avalonix.Model.Media.Track;

[NotMapped]
public class Track
{
    public TrackData TrackData { get; set; } = null!;
    [JsonIgnore] public TrackMetadata Metadata = new();
    
    public Track()
    {
    }

    public Track(string path) => TrackData = new TrackData(path);

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}