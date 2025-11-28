using System;

namespace Avalonix.Model.Media.Track;

public class Track
{
    public readonly TrackMetadata Metadata = new();
    public readonly TrackData TrackData;

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