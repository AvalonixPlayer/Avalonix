using System.Collections.Generic;
using System.Linq;

namespace Avalonix.Services.Media.Album;

public record AlbumMetadata
{
    public string AlbumName { get; private set; }
    public string? ArtistName { get; private set; }
    public string[]? Genres { get; private set; }
    public byte[]? Cover { get; private set; }

    public AlbumMetadata(List<string> tracksPaths)
    {
        var tracks = tracksPaths.Select(trackPath => new Track.Track(trackPath)).ToList();
        FillMetadata(tracks);
    }

    private void FillMetadata(List<Track.Track> tracks)
    {
        foreach (var track in tracks)
        {
            track.Metadata.Init(track.TrackData.Path);
            track.Metadata.FillTrackMetaData();
        }
        AlbumName = tracks[0].Metadata.Album;
        var tracksMetadata = tracks.Select(x => x.Metadata).ToList();
        foreach (var trackMetadata in
                 tracksMetadata.Where(trackMetadata => !string.IsNullOrEmpty(trackMetadata.Artist)))
        {
            ArtistName = trackMetadata.Artist;
            break;
        }

        foreach (var trackMetadata in tracksMetadata.Where(trackMetadata => !string.IsNullOrEmpty(trackMetadata.Genre)))
        {
            Genres = [trackMetadata.Genre ?? "none"];
            break;
        }

        foreach (var trackMetadata in tracksMetadata.Where(trackMetadata => !string.IsNullOrEmpty(trackMetadata.Genre)))
        {
            Genres = [trackMetadata.Genre ?? "none"];
            break;
        }

        foreach (var trackMetadata in tracksMetadata.Where(trackMetadata => !string.IsNullOrEmpty(trackMetadata.Genre)))
        {
            Cover = trackMetadata.Cover;
            break;
        }
    }
}