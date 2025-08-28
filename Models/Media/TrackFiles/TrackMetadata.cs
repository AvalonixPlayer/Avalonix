using System;
using System.Linq;
using TagLib;

namespace Avalonix.Models.Media.TrackFiles;

public struct TrackMetadata
{
    public string TrackName { get; private set; }
    public string? Album { get; private set; }
    public string? Artist { get; private set; }
    public string? Genre { get; private set; }
    public short? Year { get; private set; }
    public string? Lyric { get; private set; }
    public TimeSpan Duration { get; private set; }
    public byte[]? Cover { get; set; }

    public TrackMetadata(string path) =>
        FillTrackMetaData(path);

    private void FillTrackMetaData(string Path)
    {
        var track = File.Create(Path)!;
        TrackName = track.Tag!.Title ?? "Song";
        Album = track.Tag!.Album!;
        Artist = track.Tag!.FirstPerformer!;
        Genre = track.Tag!.FirstGenre!;
        Year = (short)track.Tag!.Year;
        Lyric = track.Tag!.Lyrics!;
        Duration = track.Properties!.Duration!;
        Cover = track.Tag!.Pictures!.FirstOrDefault(p => p.Type == PictureType.FrontCover)!.Data!.Data!;
    }
}