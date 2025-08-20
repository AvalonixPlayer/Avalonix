using System;
using System.IO;
using Avalonia.Media;
using System.Text.Json;
using System.Text.Json.Serialization;
using File = TagLib.File;

namespace Avalonix.API;

public struct TrackMetadata(
    string trackName,
    string? album,
    string? artist,
    string? genre,
    short? year,
    string? lyric,
    string? duration)
{
    public string TrackName => trackName;
    public string? Album => album;
    public string? Artist => artist;
    public string? Genre => genre;
    public short? Year => year;
    public string? Lyric => lyric;
    public string? Duration => duration;
}

public struct TrackData(
    string trackPath)
{
    public string TrackPath => trackPath;
}

public class Track(string path)
{
    [JsonIgnore] public TrackMetadata TrackMetadata => GetTrackMetaData();
    public string Path => path;
    public TrackData TrackData => GetTrackData();

    public TrackMetadata GetTrackMetaData()
    {
        string? trackName;
        string? album;
        string? artist;
        string? genre;
        short? year;
        string? lyric;
        string? duration;
        
        using (var track = File.Create(TrackData.TrackPath))
        {
            trackName = track.Tag.Title;
            album = track.Tag.Album;
            artist = track.Tag.FirstPerformer;
            genre = track.Tag.FirstGenre;
            year = (short)track.Tag.Year;
            lyric = track.Tag.Lyrics;
            duration = track.Tag.Length;
        }

        return new TrackMetadata(trackName, album, artist, genre, year,lyric,duration);
    }

    public TrackData GetTrackData()
    {
        return new TrackData(Path);
    }
    
}