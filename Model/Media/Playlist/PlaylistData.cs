using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Avalonix.Model.Media.Playlist;

public class PlaylistData
{
    private List<Track.Track> _tracks = [];

    [Key] public string Name { get; set; }

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

    [JsonInclude] public DateTime? LastListen { get; set; }
    [JsonInclude] public int? Rarity { get; set; } = 0;
    [JsonInclude] public bool ObserveDirectory { get; set; } = true;
    [JsonInclude] public string? ObservingDirectory { get; set; }

    private void SetName()
    {
    }

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

    private void TracksFiltration()
    {
        _tracks = _tracks.Where(track => File.Exists(track.TrackData.Path)).ToList();
    }
}