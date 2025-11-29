using System;

namespace Avalonix.Model.Media.Track;

public class TrackData
{
    public TrackData()
    {
    }

    public TrackData(string path)
    {
        Path = path;
    }

    public string Path { get; }
    public int Rarity { get; set; }
    public TimeSpan? LastListen { get; set; }
}