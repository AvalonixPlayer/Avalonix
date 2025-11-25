using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonix.Model.Media;
using Avalonix.Model.Media.MediaPlayer;
using Avalonix.Model.Media.Track;

namespace Avalonix.Services.PlayableManager;

public interface IPlayableManager
{
    CancellationTokenSource? GlobalCancellationTokenSource { get; }
    IMediaPlayer MediaPlayer { get; }
    IPlayable? PlayingPlayable { get; }
    Task<List<IPlayable>> GetPlayables();
    void StartPlayable(IPlayable playlist);
    event Action? PlayableChanged;
    event Action<bool> PlaybackStateChanged;
    event Action TrackChanged;
    event Action<bool> ShuffleChanged;
    event Action<bool> LoopChanged;
}