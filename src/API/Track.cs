using System;
using Avalonia.Media.Imaging;

namespace Avalonix.API;

[Serializable]
public struct TrackData(
    string trackName,
    string? albumArtist,
    string? artist,
    string? genre,
    int? year,
    Bitmap? cover,
    string? songText,
    int? duration,
    int? bitrate)
{
    public string TrackName => trackName;
    public string? AlbumArtist => albumArtist;
    public string? Artist => artist;
    public string? Genre => genre;
    public int? Year => year;
    public Bitmap? Cover => cover;
    public string? SongText => songText;
    public int? Duration => duration;
    public int? Bitrate => bitrate;
}