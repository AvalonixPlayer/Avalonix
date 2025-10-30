using System;
using System.Text;

namespace Avalonix.Models.Media.Track;

public class TrackData(string path)
{
    public string Path { get; } = path;
    public int Rarity { get; set; }
    public TimeSpan? LastListen { get; set; }

    public override string ToString()
    {
        var result = new StringBuilder();
        result.AppendLine($"Path: {Path}");
        result.AppendLine($"Rarity: {Rarity}");
        result.AppendLine($"LastListen: {LastListen}");
        return result.ToString();
    }
}