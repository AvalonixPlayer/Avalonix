using System;
using TagLib;

namespace Avalonix.API;

[Serializable]
public record struct TrackData(
    string trackPath,
    string trackName,
    string? album,
    string? artist,
    string? genre,
    int? year,
    string? lyric,
    string? duration,
    IPicture? cover)
{
    public string TrackPath => trackPath;
    public string TrackName => trackName;
    public string? Album => album;
    public string? Artist => artist;
    public string? Genre => genre;
    public int? Year => year;
    public string? Lyric => lyric;
    public string? Duration => duration;
    public IPicture? Cover => cover;
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
        int? year;
        IPicture? cover;
        string? lyric;
        string? duration;
        using (var track = File.Create(path))
        {
            trackName = track.Tag.Title;
            album = track.Tag.Album;
            artist = track.Tag.FirstPerformer;
            genre = track.Tag.FirstGenre;
            year = (int)track.Tag.Year;
            cover = track.Tag.Pictures.Length > 0 ? track.Tag.Pictures[0] : null!;
            lyric = track.Tag.Lyrics;
            duration = track.Tag.Length;
        }

        return new TrackData(path, trackName, album, artist, genre, year,lyric,duration, cover);
    }
}