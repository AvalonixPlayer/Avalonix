namespace Avalonix.AvalonixAPI;
public class Song()
{
    public required string Title {
        get;
        set;
    }
    public required string FilePath {
        get;
        set;
    }
    public TrackInfo TrackInfo {
        get;
        set;
    }
    public AlbumInfo AlbumInfo {
        get;
        set;
    }
    public AdditionalMetadata AdditionalMetadata {
        get;
        set;
    }

    public override string ToString() =>
    $"{TrackInfo.Artist} - {Title} ({AlbumInfo.Album}, {AlbumInfo.Year}) " +
    $"[Track {TrackInfo.TrackNumber}/{AlbumInfo.TotalTracks}, " +
    $"Disc {TrackInfo.DiscNumber}/{AlbumInfo.TotalDiscs}]";

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

        AdditionalMetadata = new AdditionalMetadata
        {
            Comment = file.Tag.Comment,
            Publisher = file.Tag.Publisher,
            Rarity = 1.0f,
        };
    }
}

public struct TrackInfo
{
    public string? Artist {
        get;
        init;
    }
    public int? TrackNumber {
        get;
        init;
    }
    public int? DiscNumber {
        get;
        init;
    }
    public string? Genre {
        get;
        set;
    }
    public string? Lyrics {
        get;
        set;
    }
    public string? Composer {
        get;
        set;
    }
}

public struct AlbumInfo
{
    public string? Album {
        get;
        set;
    }
    public string? AlbumArtist {
        get;
        set;
    }
    public int? Year {
        get;
        set;
    }
    public int? TotalTracks { get; set; } 

    public int? TotalDiscs { get; set; }
    public string? CoverArtPath { get; set; }
}

public struct AdditionalMetadata()
{
    public string Comment { get; set; } = null!;
    public string Publisher { get; set; } = null!;

    public float Rarity { get; set; } 
    public float? Rating { get; set; } 
}