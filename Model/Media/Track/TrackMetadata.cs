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
    [JsonInclude]public string? TrackName { get; private set; }
    [JsonInclude]public string? Album { get; private set; }
    [JsonInclude]public string? MediaFileFormat { get; private set; }
    [JsonInclude]public string? Artist { get; private set; }
    [JsonInclude]public string? Genre { get; private set; }
    [JsonInclude]public uint? Year { get; private set; }
    [JsonInclude]public string? Lyric { get; private set; }
    [JsonInclude]public TimeSpan Duration { get; private set; }
    [JsonInclude]public byte[]? Cover { get; private set; }

    public Task FillTrackMetaData(string path)
    {
        var track = File.Create(path);

        var tag = track.Tag!;

        TrackName = tag.Title ?? Path.GetFileNameWithoutExtension(path);
        MediaFileFormat = Path.GetExtension(path);
        Album = tag.Album!;
        Artist = tag.FirstPerformer;
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

    public Task FillBasicTrackMetaData(string path)
    {
        var track = File.Create(path);
        var tag = track.Tag!;

        TrackName = tag.Title ?? Path.GetFileNameWithoutExtension(path);
        Artist = tag.FirstPerformer;
        Album = tag.Album;

        MetadataLoaded?.Invoke();
        return Task.CompletedTask;
    }

    [Obsolete("Obsolete")]
    public void RewriteTags(string path, string title, string album, string? artist, string? genre, int year,
        string lyric,
        byte[]? cover)
    {
        using var file = File.Create(path);
        file.Tag.Title = title;
        file.Tag.Album = album;
        if (artist != null)
            file.Tag.Artists = [artist];
        if (genre != null)
            file.Tag.Genres = [genre];
        file.Tag.Year = (uint)year;
        file.Tag.Lyrics = lyric;

        if (cover != null)
        {
            file.Tag.Pictures =
            [
                new Picture
                {
                    Data = new ByteVector(cover)
                }
            ];
        }

        file.Save();
        FillTrackMetaData(path);
        MetadataEdited?.Invoke();
    }

    public static string ToHumanFriendlyString(TimeSpan timeSpan)
    {
        if (timeSpan.Hours > 0)
            return $"{timeSpan.Hours}H {timeSpan.Minutes}M {timeSpan.Seconds}S";
        return timeSpan.Minutes > 0 ? $"{timeSpan.Minutes}M {timeSpan.Seconds}S" : $"{timeSpan.Seconds}S";
    }
}