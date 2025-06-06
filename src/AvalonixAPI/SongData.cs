using System;

namespace AvalonixAPI
{
    public class SongData(string title, string filePath, TimeSpan? duration = null)
    {
        public string Title { get; set; } = title;
        public string FilePath { get; set; } = filePath;
        public TimeSpan? Duration { get; set; } = duration;
        public TrackInfo TrackInfo { get; set; } = new();
        public AlbumInfo AlbumInfo { get; set; } = new();
        public AdditionalMetadata ExtraMetadata { get; set; } = new();

        public override string ToString()
        {
            return $"{TrackInfo.Artist} - {Title} ({AlbumInfo.Album}, {AlbumInfo.Year}) " +
                   $"[Track {TrackInfo.TrackNumber}/{AlbumInfo.TotalTracks}, " +
                   $"Disc {TrackInfo.DiscNumber}/{AlbumInfo.TotalDiscs}]";
        }
    }

    public struct TrackInfo
    {
        public string? Artist { get; set; }
        public int? TrackNumber { get; set; }
        public int? DiscNumber { get; set; }
        public string? Genre { get; set; }
        public string? Lyrics { get; set; }
        public string? Composer { get; set; }
    }

    public struct AlbumInfo
    {
        public string? Album { get; set; }
        public string? AlbumArtist { get; set; }
        public int? Year { get; set; }
        public int? TotalTracks { get; set; }
        public int? TotalDiscs { get; set; }
        public string? CoverArtPath { get; set; }
    }

    public struct AdditionalMetadata
    {
        public string? Comment { get; set; }
        public string? Publisher { get; set; }
    }
}