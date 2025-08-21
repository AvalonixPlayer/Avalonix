using System.Text.Json.Serialization;
using TagLib;

namespace Avalonix.API;

public class Track
{
    public TrackData TrackData { get; set; }
    [JsonIgnore] public TrackMetadata Metadata => new TrackMetadata(TrackData.Path);

    [JsonConstructor]
    public Track()
    {
    }

    public Track(string path) => TrackData = new TrackData(path);
}

public struct TrackData
{
    [JsonInclude] public string Path { get; set; }

    public TrackData(string path) => Path = path;
}

public struct TrackMetadata
{
    public string TrackName { get; private set; }
    public string? Album { get; private set; }
    public string? Artist { get; private set; }
    public string? Genre { get; private set; }
    public short? Year { get; private set; }
    public string? Lyric { get; private set; }
    public string? Duration { get; private set; }

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
            Duration = track.Tag.Length;
        }
    }
}