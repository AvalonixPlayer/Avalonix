using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Avalonix.Models.Media.Playlist;

public class PlaylistData
{
    public List<Track.Track> Tracks
    {
        get
        {
            if (ObserveDirectory)
                UpdateTracksByDirectoryObserving();
            return FiltratedTracks();
        }
        set { }
    }

    public TimeSpan? PlaylistDuration =>
        Tracks.Aggregate(TimeSpan.Zero, (current, track) => current + track.Metadata.Duration);

    public DateTime? LastListen { get; set; } = null!;
    public int? Rarity { get; set; } = 0;
    public bool ObserveDirectory { get; set; } = false;
    public string? ObservingDirectory { get; set; } = null!;

    private void UpdateTracksByDirectoryObserving()
    {
        if (!Directory.Exists(ObservingDirectory)) return;

        var extensions = "*.mp3;*.flac;*.m4a;*.wav;*.waw";
        var files = Directory.EnumerateFiles(ObservingDirectory, extensions, SearchOption.AllDirectories);

        var result = new List<Track.Track>();

        var currentPaths = Tracks.Select(track => track.TrackData.Path).ToList();

        foreach (var file in files)
            if (!currentPaths.Contains(file))
                Tracks.Add(new Track.Track(file));
        Tracks = result;
    }

    private void TracksFiltration() =>
        Tracks = Tracks.Where(track => File.Exists(track.TrackData.Path)).ToList();

    private List<Track.Track> FiltratedTracks() =>
        Tracks.Where(track => File.Exists(track.TrackData.Path)).ToList();
}