using System;
using System.Text;

namespace Avalonix.Models.Media.TrackFiles;

public struct TrackData(string path)
{
    public string Path { get; set; } = path;
    public int Rarity { get; set; } = 0;
    public TimeSpan? LastListen { get; set; } = null!;

    public override string ToString()
    {
        var result = new StringBuilder();
        result.AppendLine($"Path: {Path}");
        result.AppendLine($"Rarity: {Rarity}");
        result.AppendLine($"LastListen: {LastListen}");
        return result.ToString();
    }
}