using System.Collections.Generic;

namespace Avalonix.Services.Media.Album;

public record AlbumData
{
    public readonly List<Track.Track> Tracks = [];

    public AlbumData(List<string> tracksPaths)
    {
        FillTracks(tracksPaths);
    }
    
    private void FillTracks(List<string> tracksPaths)
    {
        foreach (var trackPath in tracksPaths)
            Tracks.Add(new Track.Track(trackPath));
    }
}