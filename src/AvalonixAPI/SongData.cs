using System;

namespace AvalonixAPI
{
    public class SongData(string title, string filePath, TimeSpan? duration = null)
    {
        public string Title { get; set; } = title;
        public string FilePath { get; set; } = filePath;
        public TimeSpan? Duration { get; set; } = duration;
        private TrackInfo TrackInfo { get; set; } = new();
        private AlbumInfo AlbumInfo { get; set; } = new();
        public AdditionalMetadata ExtraMetadata { get; set; } = new();

        public override string ToString()
        {
            return $"{TrackInfo.Artist} - {Title} ({AlbumInfo.Album}, {AlbumInfo.Year}) " +
                   $"[Track {TrackInfo.TrackNumber}/{AlbumInfo.TotalTracks}, " +
                   $"Disc {TrackInfo.DiscNumber}/{AlbumInfo.TotalDiscs}]";
        }

        public void ExtractMetadata(string songPath)
        {
            var file = TagLib.File.Create(songPath);
            TrackInfo = new TrackInfo
                {
                    Artist = file.Tag.FirstPerformer,
                    TrackNumber = (int?)file.Tag.Track,
                    DiscNumber = (int?)file.Tag.Disc,
                    Genre = file.Tag.FirstGenre,
                    Lyrics = file.Tag.Lyrics,
                    Composer = file.Tag.FirstComposer
                };

                AlbumInfo = new AlbumInfo
                {
                    Album = file.Tag.Album,
                    AlbumArtist = file.Tag.FirstAlbumArtist,
                    Year = (int?)file.Tag.Year,
                    TotalTracks = (int?)file.Tag.TrackCount,
                    TotalDiscs = (int?)file.Tag.DiscCount,
                    CoverArtPath = null
                };

                ExtraMetadata = new AdditionalMetadata
                {
                    Comment = file.Tag.Comment,
                    Publisher = file.Tag.Publisher
                };

                Duration ??= file.Properties.Duration;
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