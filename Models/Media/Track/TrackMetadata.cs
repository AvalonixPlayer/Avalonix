using System;
using System.IO;
using System.Text;
using File = TagLib.File;

namespace Avalonix.Models.Media.Track;

public record TrackMetadata
{
    public string? TrackName { get; private set; }
    public string? Album { get; private set; }
    public string? MediaFileFormat { get; private set; }
    public string? Artist { get; private set; }
    public string? Genre { get; private set; }
    public uint? Year { get; private set; }
    public string? Lyric { get; private set; }
    public TimeSpan Duration { get; private set; }
    public byte[]? Cover { get; private set; }
    private readonly string _path;

    public TrackMetadata(string path)
    {
        _path = path;
        FillTrackMetaData();
    }

    private void FillTrackMetaData()
    {
        var track = File.Create(_path);
        TrackName = track.Tag!.Title ?? Path.GetFileNameWithoutExtension(_path);
        MediaFileFormat = Path.GetExtension(_path);
        Album = track.Tag!.Album!;
        Artist = track.Tag!.FirstPerformer!;
        Genre = track.Tag!.FirstGenre!;
        Year = track.Tag!.Year;
        Lyric = track.Tag!.Lyrics!;
        Duration = track.Properties!.Duration;
        if (track.Tag.Pictures is not { Length: > 0 }) return;
        var picture = track.Tag.Pictures[0];
        if (picture != null && picture.Data != null && picture.Data.Data is { Length: > 0 })
            Cover = picture.Data.Data;
    }

    [Obsolete("Obsolete")]
    public void RewriteTags(string title, string album, string artist, string genre, int year, string lyric, byte[]? cover)
    {
        using var file = File.Create(_path);
        file.Tag.Title = title;
        file.Tag.Album = album;
        file.Tag.Artists = [artist];
        file.Tag.Genres = [genre];
        file.Tag.Year = (uint)year;
        file.Tag.Lyrics = lyric;
            
        if (cover != null)
        {
            file.Tag.Pictures = [];
            var picture = new TagLib.Picture
            {
                Type = TagLib.PictureType.FrontCover,
                Description = "Cover",
                Data = new TagLib.ByteVector(cover)
            };
            file.Tag.Pictures = [picture];
        }
        
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
        result.AppendLine($"Cover: {Cover == null}");
        return result.ToString();
    }
}