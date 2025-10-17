using System;
using System.Collections.Generic;
using System.Linq;
using Avalonix.Models.UserSettings;

namespace Avalonix.Models.Media.Playlist;

public class PlayQueue(Settings settings, Random random)
{
    public int PlayingIndex { get; set; }
    public List<Track.Track> Tracks { get; private set; }
    private Settings _settings = settings;

    public void FillQueue(PlaylistData playlistData)
    {
        PlayingIndex = 0;
        Tracks = playlistData.Tracks;
        
        if (Settings.Avalonix.Playlists.Shuffle)
            Tracks = Tracks.OrderBy(_ => random.Next()).ToList();
    }
}