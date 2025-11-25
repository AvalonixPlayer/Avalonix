using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

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

    [Key] public string Name { get; set; }

    public string Path { get; }
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