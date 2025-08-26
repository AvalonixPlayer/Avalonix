using System;
using System.Linq;
using System.Text.Json.Serialization;
using TagLib;

namespace Avalonix.Models.Media;

public class Track
{
    public TrackData TrackData;
    [JsonIgnore] public TrackMetadata Metadata => new(TrackData.Path);

    [JsonConstructor]
    public Track()
    {
    }


    public Track(string path) => TrackData = new(path);

    public void IncreaseRarity(int rarity) => TrackData.Rarity += rarity;

    public void UpdateLastListenDate() => TrackData.LastListen = DateTime.Now.TimeOfDay;
}

public struct TrackData(string path)
{
    public string Path { get; set; } = path;
    public int Rarity { get; set; } = 0;
    public TimeSpan? LastListen { get; set; } = null!;
}

public struct TrackMetadata
{
    public string TrackName { get; private set; }
    public string? Album { get; private set; }
    public string? Artist { get; private set; }
    public string? Genre { get; private set; }
    public short? Year { get; private set; }
    public string? Lyric { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public byte[]? Cover { get; set; }

    public TrackMetadata(string Path) => FillTrackMetaData(Path);

    public void FillTrackMetaData(string Path)
    {
        using (var track = File.Create(Path))
        {
            TrackName = track.Tag.Title;
            Album = track.Tag.Album;
            Artist = track.Tag.FirstPerformer;
            Genre = track.Tag.FirstGenre;
            Year = (short)track.Tag.Year;
            Lyric = track.Tag.Lyrics;
            Duration = track.Properties.Duration;
            Cover = track.Tag.Pictures.FirstOrDefault(p => p.Type == PictureType.FrontCover).Data.Data;
        }
    }
}