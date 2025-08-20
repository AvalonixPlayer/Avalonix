using System;
using TagLib;

namespace Avalonix.API;

[Serializable]
public struct TrackData(
    string trackPath,
    string trackName,
    string? album,
    string? artist,
    string? genre,
    short? year,
    string? lyric,
    string? duration)
{
    public string TrackPath => trackPath;
    public string TrackName => trackName;
    public string? Album => album;
    public string? Artist => artist;
    public string? Genre => genre;
    public short? Year => year;
    public string? Lyric => lyric;
    public string? Duration => duration;
}

public class Track(string path)
{
    public TrackData TrackData => GetTrackData();

    public TrackData GetTrackData()
    {
        string? trackName;
        string? album;
        string? artist;
        string? genre;
        short? year;
        string? lyric;
        string? duration;
        
        using (var track = File.Create(path))
        {
            trackName = track.Tag.Title;
            album = track.Tag.Album;
            artist = track.Tag.FirstPerformer;
            genre = track.Tag.FirstGenre;
            year = (short)track.Tag.Year;
            lyric = track.Tag.Lyrics;
            duration = track.Tag.Length;
        }

        return new TrackData(path, trackName, album, artist, genre, year,lyric,duration);
    }
}