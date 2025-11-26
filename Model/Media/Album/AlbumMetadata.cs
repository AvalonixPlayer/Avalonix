using System.Collections.Generic;
using System.Linq;

namespace Avalonix.Model.Media.Album;

public record AlbumMetadata
{
    public AlbumMetadata(List<string> tracksPaths)
    {
        var tracks = tracksPaths.Select(trackPath => new Track.Track(trackPath)).ToList();
        FillMetadata(tracks);
    }

    public string AlbumName { get; private set; } = string.Empty;
    public string? ArtistName { get; private set; }

    private void FillMetadata(List<Track.Track> tracks)
    {
        foreach (var track in tracks)
        {
            track.Metadata.Init(track.TrackData.Path);
            track.Metadata.FillBasicTrackMetaData();
        }

        AlbumName = tracks[0].Metadata.Album ?? "none";
        var tracksMetadata = tracks.Select(x => x.Metadata).ToList();
        foreach (var trackMetadata in
                 tracksMetadata.Where(trackMetadata => !string.IsNullOrEmpty(trackMetadata.Artist)))
        {
            ArtistName = trackMetadata.Artist;
            break;
        }
    }
}