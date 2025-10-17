using System;
using System.Collections.Generic;
using System.Linq;

namespace Avalonix.Models.Media.Playlist;

public class PlaylistData
{
    public List<Track.Track> Tracks { get; set; } = [];

    public TimeSpan? PlaylistDuration =>
        Tracks.Aggregate(TimeSpan.Zero, (current, track) => current + track.Metadata.Duration);

    public DateTime? LastListen { get; set; } = null!;
    public int? Rarity { get; set; } = 0;
}