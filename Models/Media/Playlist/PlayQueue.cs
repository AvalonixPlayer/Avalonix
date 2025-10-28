using System;
using System.Collections.Generic;
using System.Linq;
using Avalonix.Models.UserSettings;
using Avalonix.Models.UserSettings.AvalonixSettingsFiles;

namespace Avalonix.Models.Media.Playlist;

public class PlayQueue(PlaySettings settings, Random random)
{
    public int PlayingIndex { get; set; }
    public List<Track.Track> Tracks { get; private set; } = null!;
    private PlaySettings Settings => settings;

    public void FillQueue(PlaylistData playlistData)
    {
        PlayingIndex = 0;
        Tracks = playlistData.Tracks;
        
        if (Settings.Shuffle)
            Tracks = Tracks.OrderBy(_ => random.Next()).ToList();
    }
}