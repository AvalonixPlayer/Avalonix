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

    public void FillQueue(List<Track.Track> tracks)
    {
        PlayingIndex = 0;
        Tracks = tracks;
        
        if (Settings.Shuffle)
            Tracks = Tracks.OrderBy(_ => random.Next()).ToList();
    }
}