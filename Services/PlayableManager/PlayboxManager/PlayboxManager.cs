using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Track;
using Avalonix.Services.DiskManager;
using Avalonix.Services.SettingsManager;
using Microsoft.Extensions.Logging;

namespace Avalonix.Services.PlayableManager.PlayboxManager;

public class PlayboxManager(
    IMediaPlayer player,
    ISettingsManager settingsManager,
    IDiskManager diskManager,
    IMediaPlayer mediaPlayer) : IPlayboxManager
{
    public IMediaPlayer MediaPlayer => mediaPlayer;
    public IPlayable? PlayingPlayable { get; set; }
    public Track? CurrentTrack { get; }
    
    public Task<List<IPlayable>> GetPlaylists()
    {
        throw new NotImplementedException();
    }

    public event Action? PlayableChanged;
    public event Action<bool>? PlaybackStateChanged;
    public event Action? TrackChanged;
    public event Action<bool>? ShuffleChanged;
    public event Action<bool>? LoopChanged;
}