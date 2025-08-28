using System;

namespace Avalonix.Models.Media.TrackFiles;

public struct TrackData(string path)
{
    public string Path { get; set; } = path;
    public int Rarity { get; set; } = 0;
    public TimeSpan? LastListen { get; set; } = null!;
}

