using System;
using System.IO;
using System.Linq;
using System.Text;
using TagLib;
using File = TagLib.File;

namespace Avalonix.Models.Media.TrackFiles;

public struct TrackMetadata
{
    public string? TrackName { get; set; }
    public string? Album { get; set; }
    public string? MediaFileFormat { get; set; }
    public string? Artist { get; set; }
    public string? Genre { get; set; }
    public uint? Year { get; set; }
    public string? Lyric { get; set; }
    public TimeSpan Duration { get; set; }
    public byte[]? Cover { get; set; }
    private string _path;

    public TrackMetadata(string path)
    {
        _path = path;
        FillTrackMetaData();
    }
        

    private void FillTrackMetaData()
    {
        Console.WriteLine(1);
        var track = File.Create(_path)!;
        TrackName = track.Tag!.Title;
        MediaFileFormat = Path.GetExtension(_path);
        Album = track.Tag!.Album!;
        Artist = track.Tag!.FirstPerformer!;
        Genre = track.Tag!.FirstGenre!;
        Year = track.Tag!.Year;
        Lyric = track.Tag!.Lyrics!;
        Duration = track.Properties!.Duration;
        //Cover = track.Tag!.Pictures!.FirstOrDefault(p => p.Type == PictureType.FrontCover)!.Data!.Data!;
    }

    public void RewriteTags(TrackMetadata newMetadata)
    {
        var file = File.Create(_path)!;
        var tag = file.Tag;
        tag.Album = newMetadata.Album!;
        tag.Performers = [newMetadata.Artist!];
        tag.Title = newMetadata.TrackName!;
        tag.Genres = [newMetadata.Genre];
        tag.Lyrics = newMetadata.Lyric;
        tag.Year = newMetadata.Year ?? 0;
        //tag.Pictures = newMetadata.Cover;
        file.Save();
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