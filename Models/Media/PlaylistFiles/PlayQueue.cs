using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonix.Models.Media.TrackFiles;
using Avalonix.Models.UserSettings;

namespace Avalonix.Models.Media.PlaylistFiles;

public class PlayQueue
{
    public int PlayingIndex { get; set; }
    public List<Track> Tracks { get; private set; }
    private Settings _settings;
    private readonly Random _random;
    public PlayQueue(Settings settings, Random random)
    {
        _settings = settings;
        _random = random;
    }

    public void FillQueue(PlaylistData playlistData)
    {
        PlayingIndex = 0;
        Tracks = playlistData.Tracks;
        
        if (Settings.Avalonix.Playlists.Shuffle)
            Tracks = Tracks.OrderBy(_ => _random.Next()).ToList();
    }
}