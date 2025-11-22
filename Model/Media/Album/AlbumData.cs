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
        {
            var track = new Track.Track(trackPath);
            track.Metadata.Init(trackPath);
            track.Metadata.FillTrackMetaData();
            Tracks.Add(track);
        }
    }
}