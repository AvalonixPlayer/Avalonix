using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TagLib;
using File = TagLib.File;

namespace Avalonix.Model.Media.Track;

public record TrackMetadata
{
    [JsonIgnore] public Action? MetadataEdited;

    [JsonIgnore] public Action? MetadataLoaded;
    [JsonInclude] public string? TrackName { get; set; }
    [JsonInclude] public string? Album { get; set; }
    [JsonInclude] public string? MediaFileFormat { get; set; }
    [JsonInclude] public string? Artist { get; set; }
    [JsonIgnore] public string? Genre { get; set; }
    [JsonIgnore] public uint? Year { get; set; }
    [JsonIgnore] public string? Lyric { get; set; }
    [JsonIgnore] public TimeSpan Duration { get; set; }
    [JsonIgnore] public byte[]? Cover { get; set; }

    public Task FillPreviouslyMetaData(string path)
    {
        var track = File.Create(path);

        var tag = track.Tag!;
        TrackName = tag.Title ?? Path.GetFileNameWithoutExtension(path);
        MediaFileFormat = Path.GetExtension(path);
        Album = tag.Album!;
        Artist = tag.FirstPerformer;

        MetadataLoaded?.Invoke();
        return Task.CompletedTask;
    }

    public Task FillSecondaryMetaData(string path)
    {
        var track = File.Create(path);

        var tag = track.Tag!;
        Genre = tag.FirstGenre;
        Year = tag.Year;
        Lyric = tag.Lyrics;
        Duration = track.Properties.Duration;
        if (tag.Pictures is not { Length: > 0 })
        {
            MetadataLoaded?.Invoke();
            return Task.CompletedTask;
        }

        var picture = tag.Pictures[0];
        if (picture != null && picture.Data != null && picture.Data.Data is { Length: > 0 })
            Cover = picture.Data.Data;

        MetadataLoaded?.Invoke();
        return Task.CompletedTask;
    }

    [Obsolete("Obsolete")]
    public Task RewriteTags(string path, TrackMetadata metadata)
    {
        using var file = File.Create(path);
        file.Tag.Title = metadata.TrackName;
        file.Tag.Album = metadata.Album;
        
        file.Tag.Artists = metadata.Artist != null ? [metadata.Artist] : null;
        file.Tag.Genres = metadata.Genre != null ? [metadata.Genre] : null;
        
        file.Tag.Year = (uint)metadata.Year!;
        file.Tag.Lyrics = metadata.Lyric;


        if (metadata.Cover != null)
        {
            file.Tag.Pictures =
            [
                new Picture
                {
                    Data = new ByteVector(metadata.Cover)
                }
            ];
        }

        file.Save();
        FillPreviouslyMetaData(path);
        MetadataEdited?.Invoke();
        return Task.CompletedTask;
    }

    public static string ToHumanFriendlyString(TimeSpan timeSpan)
    {
        if (timeSpan.Hours > 0)
            return $"{timeSpan.Hours}H {timeSpan.Minutes}M {timeSpan.Seconds}S";
        return timeSpan.Minutes > 0 ? $"{timeSpan.Minutes}M {timeSpan.Seconds}S" : $"{timeSpan.Seconds}S";
    }
}