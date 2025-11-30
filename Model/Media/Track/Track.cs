using System;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Track;

public class Track
{
    public TrackMetadata Metadata = new();
    public TrackData TrackData;

    public Track(string path)
    {
        TrackData = new TrackData(path);
    }

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}