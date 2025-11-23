using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Avalonix.Model.Media.Track;

public class TrackData
{
    [Key]
    public string Name { get; set; }
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

    public override string ToString()
    {
        var result = new StringBuilder();
        result.AppendLine($"Path: {Path}");
        result.AppendLine($"Rarity: {Rarity}");
        result.AppendLine($"LastListen: {LastListen}");
        return result.ToString();
    }
}
