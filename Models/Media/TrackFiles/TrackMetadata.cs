using System;
using System.IO;
using System.Linq;
using System.Text;
using TagLib;
using File = TagLib.File;

namespace Avalonix.Models.Media.TrackFiles;

public struct TrackMetadata
{
    public string? TrackName { get; private set; }
    public string? Album { get; private set; }
    public string? MediaFileFormat { get; private set; }
    public string? Artist { get; private set; }
    public string? Genre { get; private set; }
    public short? Year { get; private set; }
    public string? Lyric { get; private set; }
    public TimeSpan Duration { get; private set; }
    public byte[]? Cover { get; private set; }

    public TrackMetadata(string path) =>
        FillTrackMetaData(path);

    private void FillTrackMetaData(string path)
    {
        var track = File.Create(path)!;
        TrackName = track.Tag!.Title ?? "Song";
        MediaFileFormat = Path.GetExtension(path);
        Album = track.Tag!.Album!;
        Artist = track.Tag!.FirstPerformer!;
        Genre = track.Tag!.FirstGenre!;
        Year = (short)track.Tag!.Year;
        Lyric = track.Tag!.Lyrics!;
        Duration = track.Properties!.Duration!;
        Cover = track.Tag!.Pictures!.FirstOrDefault(p => p.Type == PictureType.FrontCover)!.Data!.Data!;
    }
    
    public override string ToString()
    {
        var result = new StringBuilder();
        result.AppendLine($"TrackName: {TrackName}");
        result.AppendLine($"Album: {Album}");
        result.AppendLine($"Format: {MediaFileFormat}");
        result.AppendLine($"Artist: {Artist}");
        result.AppendLine($"Genre: {Genre}");
        result.AppendLine($"Year: {Year}");
        result.AppendLine($"Lyric: {Lyric}");
        result.AppendLine($"Duration: {Duration}");
        result.AppendLine($"Duration: {(Cover == null ? true : false)}");
        return result.ToString();
    }
}