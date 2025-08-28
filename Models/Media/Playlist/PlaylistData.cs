using System;
using System.Collections.Generic;
using System.Linq;
using Avalonix.Models.Media.TrackFiles;

namespace Avalonix.Models.Media.PlaylistFiles;

public struct PlaylistData()
{
    public List<Track> Tracks { get; init; } = [];
    public DateTime? LastListen { get; set; } = null!;

    public TimeSpan? PlaylistDuration => Tracks.Aggregate(TimeSpan.Zero, (current, track) => current + track.Metadata.Duration);
}