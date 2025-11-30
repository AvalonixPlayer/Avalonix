using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avalonix.Model.Media.Album;

public record AlbumMetadata
{
    public AlbumMetadata(List<Track.Track> tracks)
    {
        FillMetadata(tracks).GetAwaiter();
    }

    public string AlbumName { get; private set; } = string.Empty;
    public string? ArtistName { get; private set; }

    private async Task FillMetadata(List<Track.Track> tracks)
    {
        foreach (var track in tracks)
            await track.FillTrackMetaData();

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