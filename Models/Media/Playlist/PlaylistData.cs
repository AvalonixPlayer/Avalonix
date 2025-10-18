using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Avalonix.Models.Media.Playlist;

public class PlaylistData
{
    private List<Track.Track> _tracks = [];

    public List<Track.Track> Tracks
    {
        get
        {
            if (ObserveDirectory)
                UpdateTracksByDirectoryObserving();
            TracksFiltration();
            return _tracks;
        }
        set => _tracks = value;
    }

    public TimeSpan? PlaylistDuration =>
        Tracks.Aggregate(TimeSpan.Zero, (current, track) => current + track.Metadata.Duration);

    public DateTime? LastListen { get; set; } = null!;
    public int? Rarity { get; set; } = 0;
    public bool ObserveDirectory { get; set; } = true;
    public string? ObservingDirectory { get; set; } = @"F:\Плейлисты\Linkin park";

    private void UpdateTracksByDirectoryObserving()
    {
        if (!Directory.Exists(ObservingDirectory)) return;

        var extensions = new[] { "*.mp3", "*.flac", "*.m4a", "*.wav", "*.waw" };
        var files = extensions.SelectMany(ext => 
            Directory.EnumerateFiles(ObservingDirectory, ext, SearchOption.AllDirectories));

        var currentPaths = _tracks.Select(track => track.TrackData.Path).ToList();

        foreach (var file in files)
            if (!currentPaths.Contains(file))
                _tracks.Add(new Track.Track(file));
    }

    private void TracksFiltration() =>
        _tracks = _tracks.Where(track => File.Exists(track.TrackData.Path)).ToList();

    private List<Track.Track> FiltratedTracks() =>
        _tracks.Where(track => File.Exists(track.TrackData.Path)).ToList();
}